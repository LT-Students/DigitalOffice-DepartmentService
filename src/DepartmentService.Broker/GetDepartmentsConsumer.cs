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
    private readonly ICacheNotebook _cache;
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
      ICacheNotebook cache,
      IOptions<RedisConfig> redisConfig,
      IRedisHelper redisHelper)
    {
      _departmentRepository = departmenrRepository;
      _departmentDataMapper = departmentDataMapper;
      _cache = cache;
      _redisConfig = redisConfig;
      _redisHelper = redisHelper;
    }

    public async Task Consume(ConsumeContext<IGetDepartmentsRequest> context)
    {
      List<DepartmentData> departmentsData = await GetDepartmentsAsync(context.Message);

      object result = OperationResultWrapper.CreateResponse(
        (_) => IGetDepartmentsResponse.CreateObj(departmentsData), context);

      await context.RespondAsync<IOperationResult<IGetDepartmentsResponse>>(result);

      if (departmentsData is not null)
      {
        List<Guid> departmentsIds = departmentsData.Select(d => d.Id).ToList();
        string key = departmentsIds.ToString();

        await _redisHelper.CreateAsync(
          Cache.Departments,
          key,
          departmentsData,
          TimeSpan.FromMinutes(_redisConfig.Value.CacheLiveInMinutes));

        _cache.Add(departmentsIds, Cache.Departments, key);

        await _redisHelper.CreateAsync(
          Cache.Departments,
          departmentsIds.GetRedisCacheHashCode(),
          departmentsData,
          TimeSpan.FromMinutes(_redisConfig.Value.CacheLiveInMinutes));
      }
    }
  }
}
