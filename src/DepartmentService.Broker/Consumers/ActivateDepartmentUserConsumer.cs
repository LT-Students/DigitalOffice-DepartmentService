using System;
using System.Threading.Tasks;
using DigitalOffice.Models.Broker.Publishing;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.DepartmentService.Broker.Consumers
{
  public class ActivateDepartmentUserConsumer : IConsumer<IActivateUserPublish>
  {
    private readonly IDepartmentUserRepository _repository;
    private readonly IGlobalCacheRepository _globalCache;
    private readonly ILogger<ActivateDepartmentUserConsumer> _logger;

    public ActivateDepartmentUserConsumer(
      IDepartmentUserRepository repository,
      IGlobalCacheRepository globalCache,
      ILogger<ActivateDepartmentUserConsumer> logger)
    {
      _repository = repository;
      _globalCache = globalCache;
      _logger = logger;
    }

    public async Task Consume(ConsumeContext<IActivateUserPublish> context)
    {
      Guid? departmentId = await _repository.ActivateAsync(context.Message);

      if (departmentId.HasValue)
      {
        await _globalCache.RemoveAsync(departmentId.Value);

        _logger.LogInformation("UserId '{UserId}' activated in departmentId '{DepartmentId}'", context.Message.UserId, departmentId);
      }
    }
  }
}
