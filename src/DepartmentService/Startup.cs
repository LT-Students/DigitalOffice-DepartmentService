using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using HealthChecks.UI.Client;
using LT.DigitalOffice.DepartmentService.Broker.Consumers;
using LT.DigitalOffice.DepartmentService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.DepartmentService.Models.Dto.Configuration;
using LT.DigitalOffice.Kernel.BrokerSupport.Configurations;
using LT.DigitalOffice.Kernel.BrokerSupport.Extensions;
using LT.DigitalOffice.Kernel.BrokerSupport.Middlewares.Token;
using LT.DigitalOffice.Kernel.Configurations;
using LT.DigitalOffice.Kernel.CustomModelBinderProviders;
using LT.DigitalOffice.Kernel.EFSupport.Extensions;
using LT.DigitalOffice.Kernel.EFSupport.Helpers;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Helpers;
using LT.DigitalOffice.Kernel.Middlewares.ApiInformation;
using LT.DigitalOffice.Kernel.RedisSupport.Configurations;
using LT.DigitalOffice.Kernel.RedisSupport.Constants;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Serilog;
using StackExchange.Redis;

namespace LT.DigitalOffice.DepartmentService
{
  public class Startup : BaseApiInfo
  {
    public const string CorsPolicyName = "LtDoCorsPolicy";
    private string redisConnStr;

    private readonly RabbitMqConfig _rabbitMqConfig;
    private readonly BaseServiceInfoConfig _serviceInfoConfig;

    public IConfiguration Configuration { get; }

    #region public methods

    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;

      _serviceInfoConfig = Configuration
        .GetSection(BaseServiceInfoConfig.SectionName)
        .Get<BaseServiceInfoConfig>();

      _rabbitMqConfig = Configuration
        .GetSection(BaseRabbitMqConfig.SectionName)
        .Get<RabbitMqConfig>();

      Version = "1.0.2.0";
      Description = "DepartmentService is an API that intended to work with Department.";
      StartTime = DateTime.UtcNow;
      ApiName = $"LT Digital Office - {_serviceInfoConfig.Name}";
    }

    public void ConfigureServices(IServiceCollection services)
    {
      services.AddCors(options =>
      {
        options.AddPolicy(
          CorsPolicyName,
          builder =>
          {
            builder
              .AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
          });
      });

      if (int.TryParse(Environment.GetEnvironmentVariable("RedisCacheLiveInMinutes"), out int redisCacheLifeTime))
      {
        services.Configure<RedisConfig>(options =>
        {
          options.CacheLiveInMinutes = redisCacheLifeTime;
        });
      }
      else
      {
        services.Configure<RedisConfig>(Configuration.GetSection(RedisConfig.SectionName));
      }

      services.Configure<TokenConfiguration>(Configuration.GetSection("CheckTokenMiddleware"));
      services.Configure<BaseRabbitMqConfig>(Configuration.GetSection(BaseRabbitMqConfig.SectionName));
      services.Configure<BaseServiceInfoConfig>(Configuration.GetSection(BaseServiceInfoConfig.SectionName));

      services.AddHttpContextAccessor();

      services
        .AddControllers()
        .AddJsonOptions(options =>
        {
          options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        })
        .AddNewtonsoftJson();

      string dbConnStr = ConnectionStringHandler.Get(Configuration);

      services.AddDbContext<DepartmentServiceDbContext>(options =>
      {
        options.UseSqlServer(dbConnStr);
      });

      services.AddHealthChecks()
        .AddRabbitMqCheck()
        .AddSqlServer(dbConnStr);

      redisConnStr = Environment.GetEnvironmentVariable("RedisConnectionString");
      if (string.IsNullOrEmpty(redisConnStr))
      {
        redisConnStr = Configuration.GetConnectionString("Redis");

        Log.Information($"Redis connection string from appsettings.json was used. " +
          $"Value '{PasswordHider.Hide(redisConnStr)}'");
      }
      else
      {
        Log.Information($"Redis connection string from environment was used. " +
          $"Value '{PasswordHider.Hide(redisConnStr)}'");
      }

      services.AddSingleton<IConnectionMultiplexer>(
        x => ConnectionMultiplexer.Connect(redisConnStr + ",abortConnect=false,connectRetry=1,connectTimeout=2000"));

      services.AddBusinessObjects();

      ConfigureMassTransit(services);

      services.AddControllers(options =>
      {
        options.ModelBinderProviders.Insert(0, new CustomModelBinderProvider());
      });
    }

    public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
    {
      app.UpdateDatabase<DepartmentServiceDbContext>();

      string error = FlushRedisDbHelper.FlushDatabase(redisConnStr, Cache.Departments);
      if (error is not null)
      {
        Log.Error(error);
      }

      app.UseForwardedHeaders();

      app.UseExceptionsHandler(loggerFactory);

      app.UseApiInformation();

      app.UseRouting();

      app.UseMiddleware<TokenMiddleware>();

      app.UseCors(CorsPolicyName);

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers().RequireCors(CorsPolicyName);

        endpoints.MapHealthChecks($"/{_serviceInfoConfig.Id}/hc", new HealthCheckOptions
        {
          ResultStatusCodes = new Dictionary<HealthStatus, int>
            {
              { HealthStatus.Unhealthy, 200 },
              { HealthStatus.Healthy, 200 },
              { HealthStatus.Degraded, 200 },
            },
          Predicate = check => check.Name != "masstransit-bus",
          ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
      });
    }

    #endregion

    #region private methods

    private (string username, string password) GetRabbitMqCredentials()
    {
      static string GetString(string envVar, string formAppsettings, string generated, string fieldName)
      {
        string str = Environment.GetEnvironmentVariable(envVar);
        if (string.IsNullOrEmpty(str))
        {
          str = formAppsettings ?? generated;

          Log.Information(
            formAppsettings == null
              ? $"Default RabbitMq {fieldName} was used."
              : $"RabbitMq {fieldName} from appsetings.json was used.");
        }
        else
        {
          Log.Information($"RabbitMq {fieldName} from environment was used.");
        }

        return str;
      }

      return (GetString("RabbitMqUsername", _rabbitMqConfig.Username, $"{_serviceInfoConfig.Name}_{_serviceInfoConfig.Id}", "Username"),
        GetString("RabbitMqPassword", _rabbitMqConfig.Password, _serviceInfoConfig.Id, "Password"));
    }

    private void ConfigureMassTransit(IServiceCollection services)
    {
      (string username, string password) = GetRabbitMqCredentials();

      services.AddMassTransit(x =>
      {
        x.AddConsumer<CreateDepartmentEntityConsumer>();
        x.AddConsumer<GetDepartmentsConsumer>();
        x.AddConsumer<DisactivateDepartmentUserConsumer>();
        x.AddConsumer<GetDepartmentUsersConsumer>();
        x.AddConsumer<SearchDepartmentsConsumer>();
        x.AddConsumer<FilterDepartmentsUsersConsumer>();

        x.UsingRabbitMq((context, cfg) =>
        {
          cfg.Host(_rabbitMqConfig.Host, "/", host =>
          {
            host.Username(username);
            host.Password(password);
          });

          ConfigureEndpoints(context, cfg);
        });

        x.AddRequestClients(_rabbitMqConfig);
      });

      services.AddMassTransitHostedService();
    }

    private void ConfigureEndpoints(
      IBusRegistrationContext context,
      IRabbitMqBusFactoryConfigurator cfg)
    {
      cfg.ReceiveEndpoint(_rabbitMqConfig.CreateDepartmentEntityEndpoint, ep =>
      {
        ep.ConfigureConsumer<CreateDepartmentEntityConsumer>(context);
      });
      cfg.ReceiveEndpoint(_rabbitMqConfig.GetDepartmentsEndpoint, ep =>
      {
        ep.ConfigureConsumer<GetDepartmentsConsumer>(context);
      });
      cfg.ReceiveEndpoint(_rabbitMqConfig.DisactivateDepartmentUserEndpoint, ep =>
      {
        ep.ConfigureConsumer<DisactivateDepartmentUserConsumer>(context);
      });
      cfg.ReceiveEndpoint(_rabbitMqConfig.GetDepartmentUsersEndpoint, ep =>
      {
        ep.ConfigureConsumer<GetDepartmentUsersConsumer>(context);
      });
      cfg.ReceiveEndpoint(_rabbitMqConfig.SearchDepartmentEndpoint, ep =>
      {
        ep.ConfigureConsumer<SearchDepartmentsConsumer>(context);
      });
      cfg.ReceiveEndpoint(_rabbitMqConfig.FilterDepartmentsEndpoint, ep =>
      {
        ep.ConfigureConsumer<FilterDepartmentsUsersConsumer>(context);
      });
    }

    #endregion
  }
}
