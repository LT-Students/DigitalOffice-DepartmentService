using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using LT.DigitalOffice.Models.Broker.Publishing.Subscriber.Department;
using MassTransit;

namespace LT.DigitalOffice.DepartmentService.Broker
{
  public class CreateDepartmentEntityConsumer : IConsumer<ICreateDepartmentEntityPublish>
  {
    private readonly IDepartmentProjectRepository _projectRepository;
    private readonly IDepartmentUserRepository _userRepository;
    private readonly IDbDepartmentProjectMapper _projectMapper;
    private readonly IDbDepartmentUserMapper _userMapper;
    private readonly IGlobalCacheRepository _globalCache;

    private async Task<bool> CreateEntityAsync(ICreateDepartmentEntityPublish request)
    {
      bool response = false;

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
        response = await _userRepository.CreateAsync(
          _userMapper.Map(
            request.UserId.Value,
            request.DepartmentId,
            request.CreatedBy))
          is not null;
      }

      return response;
    }

    public CreateDepartmentEntityConsumer(
      IDepartmentProjectRepository projectRepository,
      IDepartmentUserRepository userReposirory,
      IDbDepartmentProjectMapper projectMapper,
      IDbDepartmentUserMapper userMapper,
      IGlobalCacheRepository globalCache)
    {
      _userRepository = userReposirory;
      _projectRepository = projectRepository;
      _projectMapper = projectMapper;
      _userMapper = userMapper;
      _globalCache = globalCache;
    }

    public async Task Consume(ConsumeContext<ICreateDepartmentEntityPublish> context)
    {
      object result = OperationResultWrapper.CreateResponse(CreateEntityAsync, context.Message);

      await _globalCache.RemoveAsync(context.Message.DepartmentId);

      await context.RespondAsync<IOperationResult<bool>>(result);
    }
  }
}
