using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Kernel.RedisSupport.Configurations;
using LT.DigitalOffice.Kernel.RedisSupport.Constants;
using LT.DigitalOffice.Kernel.RedisSupport.Extensions;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using LT.DigitalOffice.Models.Broker.Models.Department;
using LT.DigitalOffice.Models.Broker.Requests.Department;
using LT.DigitalOffice.Models.Broker.Responses.Department;
using MassTransit;
using Microsoft.Extensions.Options;

namespace LT.DigitalOffice.DepartmentService.Broker.Consumers
{
  public class FilterDepartmentsUsersConsumer : IConsumer<IFilterDepartmentsRequest>
  {
    private readonly IDepartmentRepository _repository;
    private readonly IOptions<RedisConfig> _redisConfig;
    private readonly IGlobalCacheRepository _globalCache;

    public async Task<List<DepartmentFilteredData>> GetDepartmentFilteredData(IFilterDepartmentsRequest request)
    {
      List<DbDepartment> dbDepartment = await _repository.GetAsync(departmentsIds: request.DepartmentsIds);

      return dbDepartment.Select(
        pd => new DepartmentFilteredData(
          pd.Id,
          pd.Name,
          pd.Users.Select(u => u.UserId).ToList()))
        .ToList();
    }

    public FilterDepartmentsUsersConsumer(
      IDepartmentRepository repository,
      IOptions<RedisConfig> redisConfig,
      IGlobalCacheRepository globalCache)
    {
      _repository = repository;
      _redisConfig = redisConfig;
      _globalCache = globalCache;
    }

    public async Task Consume(ConsumeContext<IFilterDepartmentsRequest> context)
    {
      List<DepartmentFilteredData> departmentFilteredData = await GetDepartmentFilteredData(context.Message);

      await context.RespondAsync<IOperationResult<IFilterDepartmentsResponse>>(
        OperationResultWrapper.CreateResponse((_) => IFilterDepartmentsResponse.CreateObj(departmentFilteredData), context));

      if (departmentFilteredData is not null)
      {
        List<Guid> elementsIds = departmentFilteredData.Select(d => d.Id)
          .Concat(departmentFilteredData.SelectMany(d => d.UsersIds)).ToList();

        await _globalCache.CreateAsync(
          Cache.Departments,
          context.Message.DepartmentsIds.GetRedisCacheKey(nameof(IFilterDepartmentsRequest), context.Message.GetBasicProperties()),
          departmentFilteredData,
          context.Message.DepartmentsIds,
          TimeSpan.FromMinutes(_redisConfig.Value.CacheLiveInMinutes));
      }
    }
  }
}
