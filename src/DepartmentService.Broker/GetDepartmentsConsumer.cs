using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Configurations;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.Models.Broker.Requests.Company;
using LT.DigitalOffice.Models.Broker.Responses.Company;
using MassTransit;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace LT.DigitalOffice.DepartmentService.Broker
{
  public class GetDepartmentsConsumer : IConsumer<IGetDepartmentsRequest>
  {
    private readonly IDepartmentRepository _repository;
    private readonly IConnectionMultiplexer _cache;
    private readonly IOptions<RedisConfig> _redisConfig;
    private readonly IRedisHelper _redisHelper;

    private async Task<List<DepartmentData>> GetDepartment(IGetDepartmentsRequest request)
    {
      List<DbDepartment> dbDepartments = new();

      List<Guid> departmentsIds = request.DepartmentsIds;

      if (departmentsIds != null && departmentsIds.Any())
      {
        dbDepartments = await _repository.GetAsync(departmentsIds, true);
      }

      return dbDepartments.Select(
        d => new DepartmentData(
          d.Id,
          d.Name,
          d.Users.FirstOrDefault(u => u.Role == (int)DepartmentUserRole.Director)?.UserId,
          d.Users.Select(u => u.UserId).ToList())).ToList();
    }

    public GetDepartmentsConsumer(
      IDepartmentRepository repository,
      IConnectionMultiplexer cache,
      IOptions<RedisConfig> redisConfig,
      IRedisHelper redisHelper)
    {
      _repository = repository;
      _cache = cache;
      _redisConfig = redisConfig;
      _redisHelper = redisHelper;
    }

    public async Task Consume(ConsumeContext<IGetDepartmentsRequest> context)
    {
      List<DepartmentData> departments = await GetDepartment(context.Message);

      object departmentId = OperationResultWrapper.CreateResponse((_) => IGetDepartmentsResponse.CreateObj(departments), context);

      await context.RespondAsync<IOperationResult<IGetDepartmentsResponse>>(departmentId);

      if (departments != null)
      {
        await _redisHelper.CreateAsync(
          Cache.Departments,
          context.Message.DepartmentsIds.GetRedisCacheHashCode(),
          departments,
          TimeSpan.FromMinutes(_redisConfig.Value.CacheLiveInMinutes));
      }
    }
  }
}
