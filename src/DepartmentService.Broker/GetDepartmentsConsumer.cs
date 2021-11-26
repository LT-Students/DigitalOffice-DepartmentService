using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Configurations;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Models.Broker.Models.Department;
using LT.DigitalOffice.Models.Broker.Requests.Department;
using LT.DigitalOffice.Models.Broker.Responses.Department;
using MassTransit;
using Microsoft.Extensions.Options;

namespace LT.DigitalOffice.DepartmentService.Broker
{
  public class GetDepartmentsConsumer : IConsumer<IGetDepartmentsRequest>
  {
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IDepartmentDataMapper _departmentDataMapper;
    private readonly ICacheNotebook _cacheNotebook;
    private readonly IOptions<RedisConfig> _redisConfig;
    private readonly IRedisHelper _redisHelper;

    private async Task<List<DepartmentData>> GetDepartmentsAsync(IGetDepartmentsRequest request)
    {
      List<DbDepartment> dbDepartments = await _departmentRepository.GetAsync(request);

      return dbDepartments.Select(_departmentDataMapper.Map).ToList();
    }

    public GetDepartmentsConsumer(
      IDepartmentRepository departmenrRepository,
      IDepartmentDataMapper departmentDataMapper,
      ICacheNotebook cacheNotebook,
      IOptions<RedisConfig> redisConfig,
      IRedisHelper redisHelper)
    {
      _departmentRepository = departmenrRepository;
      _departmentDataMapper = departmentDataMapper;
      _cacheNotebook = cacheNotebook;
      _redisConfig = redisConfig;
      _redisHelper = redisHelper;
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

        if (context.Message.NewsIds is not null && context.Message.NewsIds.Any())
        {
          allGuids.AddRange(context.Message.NewsIds);
        }

        if (context.Message.ProjectsIds is not null && context.Message.ProjectsIds.Any())
        {
          allGuids.AddRange(context.Message.ProjectsIds);
        }

        if (context.Message.UsersIds is not null && context.Message.UsersIds.Any())
        {
          allGuids.AddRange(context.Message.UsersIds);
        }

        if (allGuids.Any())
        {
          string key = allGuids.GetRedisCacheHashCode();

          await _redisHelper.CreateAsync(
            Cache.Departments,
            key,
            departmentsData,
            TimeSpan.FromMinutes(_redisConfig.Value.CacheLiveInMinutes));

          _cacheNotebook.Add(departmentsData.Select(d => d.Id).ToList(), Cache.Departments, key);
        }
      }
    }
  }
}
