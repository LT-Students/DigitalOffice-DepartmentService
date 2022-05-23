using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using LT.DigitalOffice.Models.Broker.Publishing.Subscriber.Department;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.DepartmentService.Broker.Consumers
{
  public class CreateDepartmentEntityConsumer : IConsumer<ICreateDepartmentEntityPublish>
  {
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IDepartmentProjectRepository _projectRepository;
    private readonly IDepartmentUserRepository _userRepository;
    private readonly IDbDepartmentProjectMapper _projectMapper;
    private readonly IDbDepartmentUserMapper _userMapper;
    private readonly IGlobalCacheRepository _globalCache;
    private readonly ILogger<CreateDepartmentEntityConsumer> _logger;

    private async Task<bool> CreateEntityAsync(ICreateDepartmentEntityPublish request)
    {
      bool response = false;

      if (await _departmentRepository.ExistAsync(request.DepartmentId))
      {
        if (request.ProjectId.HasValue)
        {
          response = await _projectRepository.CreateAsync(
            _projectMapper.Map(
              request.ProjectId.Value,
              request.DepartmentId,
              request.CreatedBy))
            is not null;
        }
        else if (request.UserId.HasValue)
        {
          response = await _userRepository.CreateAsync(new List<DbDepartmentUser>()
          {
            _userMapper.Map(
              request.UserId.Value,
              request.DepartmentId,
              request.CreatedBy)
          });
        }
      }

      return response;
    }

    public CreateDepartmentEntityConsumer(
      IDepartmentRepository departmentRepository,
      IDepartmentProjectRepository projectRepository,
      IDepartmentUserRepository userReposirory,
      IDbDepartmentProjectMapper projectMapper,
      IDbDepartmentUserMapper userMapper,
      IGlobalCacheRepository globalCache,
      ILogger<CreateDepartmentEntityConsumer> logger)
    {
      _departmentRepository = departmentRepository;
      _userRepository = userReposirory;
      _projectRepository = projectRepository;
      _projectMapper = projectMapper;
      _userMapper = userMapper;
      _globalCache = globalCache;
      _logger = logger;
    }

    public async Task Consume(ConsumeContext<ICreateDepartmentEntityPublish> context)
    {
      if (await CreateEntityAsync(context.Message))
      {
        await _globalCache.RemoveAsync(context.Message.DepartmentId);
      }
      else
      {
        _logger.LogError(
          "Cannot create UserId: {UserId} or ProjectId: {ProjectId} for DepartmentId: {DepartmentID}.",
          context.Message.UserId,
          context.Message.ProjectId,
          context.Message.DepartmentId);
      }
    }
  }
}
