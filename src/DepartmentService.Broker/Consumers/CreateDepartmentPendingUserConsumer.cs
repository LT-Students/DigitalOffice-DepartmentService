using System;
using System.Threading.Tasks;
using DigitalOffice.Models.Broker.Publishing;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using MassTransit;

namespace LT.DigitalOffice.DepartmentService.Broker.Consumers
{
  public class CreateDepartmentPendingUserConsumer : IConsumer<ICreatePendingUserPublish>
  {
    private readonly IDepartmentUserRepository _departmentUserRepository;
    private readonly IGlobalCacheRepository _globalCache;

    public CreateDepartmentPendingUserConsumer(
      IDepartmentUserRepository departmentUserRepository,
      IGlobalCacheRepository globalCache)
    {
      _departmentUserRepository = departmentUserRepository;
      _globalCache = globalCache;
    }

    public async Task Consume(ConsumeContext<ICreatePendingUserPublish> context)
    {
      Guid? departmentId = await _departmentUserRepository.MakeUserPendingAsync(context.Message.UserId, context.Message.CreatedBy);

      if (departmentId.HasValue)
      {
        await _globalCache.RemoveAsync(departmentId.Value);
      }
    }
  }
}
