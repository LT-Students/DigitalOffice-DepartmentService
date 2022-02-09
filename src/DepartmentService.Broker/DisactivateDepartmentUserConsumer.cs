using System;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using LT.DigitalOffice.Models.Broker.Common;
using MassTransit;

namespace LT.DigitalOffice.DepartmentService.Broker
{
  public class DisactivateDepartmentUserConsumer : IConsumer<IDisactivateUserRequest>
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

    public async Task Consume(ConsumeContext<IDisactivateUserRequest> context)
    {
      Guid? departmentId = await _repository.RemoveAsync(context.Message.UserId, context.Message.ModifiedBy);

      if (departmentId.HasValue)
      {
        await _globalCache.RemoveAsync(departmentId.Value);
      }
    }
  }
}
