using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Models.Broker.Requests.Department;
using MassTransit;

namespace LT.DigitalOffice.DepartmentService.Broker
{
  public class CreateDepartmentEntityConsumer : IConsumer<ICreateDepartmentEntityRequest>
  {
    private readonly IDepartmentNewsRepository _newsRepository;
    private readonly IDepartmentProjectRepository _projectRepository;
    private readonly IDepartmentUserRepository _userRepository;
    private readonly IDbDepartmentNewsMapper _newsMapper;
    private readonly IDbDepartmentProjectMapper _projectMapper;
    private readonly IDbDepartmentUserMapper _userMapper;

    private async Task<bool> CreateEntityAsync(ICreateDepartmentEntityRequest request)
    {
      bool response = false;

      if (request.NewsId.HasValue)
      {
        response = await _newsRepository.CreateAsync(
          _newsMapper.Map(
            request.NewsId.Value,
            request.DepartmentId,
            request.CreatedBy))
          is not null;
      }
      else if (request.ProjectId.HasValue)
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
      IDepartmentNewsRepository newsRepository,
      IDepartmentProjectRepository projectRepository,
      IDepartmentUserRepository userReposirory,
      IDbDepartmentNewsMapper newsMapper,
      IDbDepartmentProjectMapper projectMapper,
      IDbDepartmentUserMapper userMapper)
    {
      _userRepository = userReposirory;
      _projectRepository = projectRepository;
      _newsRepository = newsRepository;
      _newsMapper = newsMapper;
      _projectMapper = projectMapper;
      _userMapper = userMapper;
    }

    public async Task Consume(ConsumeContext<ICreateDepartmentEntityRequest> context)
    {
      object result = OperationResultWrapper.CreateResponse(CreateEntityAsync, context.Message);

      await context.RespondAsync<IOperationResult<bool>>(result);
    }
  }
}
