using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
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
  public class GetDepartmentsConsumer : IConsumer<IGetDepartmentsRequest>
  {
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IDepartmentDataMapper _departmentDataMapper;
    private readonly IGlobalCacheRepository _globalCache;
    private readonly IOptions<RedisConfig> _redisConfig;

    private async Task<List<DepartmentData>> GetDepartmentsAsync(IGetDepartmentsRequest request)
    {
      List<DbDepartment> dbDepartments = await _departmentRepository.GetAsync(
        departmentsIds: request.DepartmentsIds,
        usersIds: request.UsersIds);

      return dbDepartments.Select(_departmentDataMapper.Map).ToList();
    }

    public GetDepartmentsConsumer(
      IDepartmentRepository departmenrRepository,
      IDepartmentDataMapper departmentDataMapper,
      IGlobalCacheRepository globalCache,
      IOptions<RedisConfig> redisConfig)
    {
      _departmentRepository = departmenrRepository;
      _departmentDataMapper = departmentDataMapper;
      _globalCache = globalCache;
      _redisConfig = redisConfig;
    }

    public async Task Consume(ConsumeContext<IGetDepartmentsRequest> context)
    {
      List<DepartmentData> departmentsData = await GetDepartmentsAsync(context.Message);

      object result = OperationResultWrapper.CreateResponse(
        (_) => IGetDepartmentsResponse.CreateObj(departmentsData), context);

      await context.RespondAsync<IOperationResult<IGetDepartmentsResponse>>(result);

      if (departmentsData is not null && departmentsData.Any())
      {
        List<Guid> allGuids = new();

        if (context.Message.UsersIds is not null && context.Message.UsersIds.Any())
        {
          allGuids.AddRange(context.Message.UsersIds);
        }

        if (context.Message.DepartmentsIds is not null && context.Message.DepartmentsIds.Any())
        {
          allGuids.AddRange(context.Message.DepartmentsIds);
        }

        if (allGuids.Any())
        {
          List<Guid> elementsIds = departmentsData.Select(d => d.Id).Concat(departmentsData.SelectMany(d => d.Users.Select(du => du.UserId))).ToList();

          await _globalCache.CreateAsync(
            Cache.Departments,
            allGuids.GetRedisCacheKey(nameof(IGetDepartmentsRequest), context.Message.GetBasicProperties()),
            departmentsData,
            elementsIds,
            TimeSpan.FromMinutes(_redisConfig.Value.CacheLiveInMinutes));
        }
      }
    }
  }
}
