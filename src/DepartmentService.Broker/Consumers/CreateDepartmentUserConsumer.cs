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
  public class CreateDepartmentUserConsumer : IConsumer<ICreateDepartmentUserPublish>
  {
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IDepartmentUserRepository _userRepository;
    private readonly IDbDepartmentUserMapper _userMapper;
    private readonly IGlobalCacheRepository _globalCache;
    private readonly ILogger<CreateDepartmentUserConsumer> _logger;

    public CreateDepartmentUserConsumer(
      IDepartmentRepository departmentRepository,
      IDepartmentUserRepository userReposirory,
      IDbDepartmentUserMapper userMapper,
      IGlobalCacheRepository globalCache,
      ILogger<CreateDepartmentUserConsumer> logger)
    {
      _departmentRepository = departmentRepository;
      _userRepository = userReposirory;
      _userMapper = userMapper;
      _globalCache = globalCache;
      _logger = logger;
    }

    public async Task Consume(ConsumeContext<ICreateDepartmentUserPublish> context)
    {
      if (await _departmentRepository.ExistAsync(context.Message.DepartmentId))
      {
        Task<bool> createUserTask = _userRepository.CreateAsync(new List<DbDepartmentUser>() { _userMapper.Map(context.Message) });

        Task<bool> updateCacheTask = _globalCache.RemoveAsync(context.Message.DepartmentId);

        await createUserTask;
        await updateCacheTask;
      }
      else
      {
        _logger.LogError(
          "Cannot create UserId: {UserId} for DepartmentId: {DepartmentID}.",
          context.Message.UserId,
          context.Message.DepartmentId);
      }
    }
  }
}
