using System;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using LT.DigitalOffice.Models.Broker.Publishing;
using MassTransit;

namespace LT.DigitalOffice.DepartmentService.Broker.Consumers
{
  public class DisactivateDepartmentUserConsumer : IConsumer<IDisactivateUserPublish>
  {
    private readonly IDepartmentUserRepository _repository;
    private readonly IGlobalCacheRepository _globalCache;

    public DisactivateDepartmentUserConsumer(
      IDepartmentUserRepository repository,
      IGlobalCacheRepository globalCache)
    {
      _repository = repository;
      _globalCache = globalCache;
    }

    public async Task Consume(ConsumeContext<IDisactivateUserPublish> context)
    {
      Guid? departmentId = await _repository.RemoveAsync(context.Message.UserId, context.Message.ModifiedBy);

      if (departmentId.HasValue)
      {
        await _globalCache.RemoveAsync(departmentId.Value);
      }
    }
  }
}
