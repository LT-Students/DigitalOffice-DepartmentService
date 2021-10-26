using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Configurations;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Models.Broker.Models.Department;
using LT.DigitalOffice.Models.Broker.Requests.Department;
using LT.DigitalOffice.Models.Broker.Responses.Department;
using MassTransit;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace LT.DigitalOffice.DepartmentService.Broker
{
  public class GetDepartmentsConsumer : IConsumer<IGetDepartmentsRequest>
  {
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IConnectionMultiplexer _cache;
    private readonly IOptions<RedisConfig> _redisConfig;
    private readonly IRedisHelper _redisHelper;

    private async Task<object> GetDepartments(IGetDepartmentsRequest request)
    {
      List<DbDepartment> dbDepartments = await _departmentRepository.GetAsync(request);

      return IGetDepartmentsResponse.CreateObj(dbDepartments.Select(
        d => new DepartmentData(
          d.Id,
          d.Name,
          directorUserId: d.Users
            ?.FirstOrDefault(du => du.Role == (int)DepartmentUserRole.Director)
            ?.UserId,
          newsIds: d.News?.Select(dn => dn.NewsId).ToList(),
          projectsIds: d.Projects?.Select(dp => dp.ProjectId).ToList(),
          usersIds: d.Users?.Select(u => u.UserId).ToList()))
        .ToList());
    }

    public GetDepartmentsConsumer(
      IDepartmentRepository departmenrRepository,
      IConnectionMultiplexer cache,
      IOptions<RedisConfig> redisConfig,
      IRedisHelper redisHelper)
    {
      _departmentRepository = departmenrRepository;
      _cache = cache;
      _redisConfig = redisConfig;
      _redisHelper = redisHelper;
    }

    public async Task Consume(ConsumeContext<IGetDepartmentsRequest> context)
    {
      object result = OperationResultWrapper.CreateResponse(GetDepartments, context.Message);

      await context.RespondAsync<IOperationResult<IGetDepartmentsResponse>>(result);

      /*if (departments != null)
      {
        await _redisHelper.CreateAsync(
          Cache.Departments,
          context.Message.DepartmentsIds.GetRedisCacheHashCode(),
          departments,
          TimeSpan.FromMinutes(_redisConfig.Value.CacheLiveInMinutes));
      }*/
    }
  }
}
