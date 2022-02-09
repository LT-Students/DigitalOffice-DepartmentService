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
      DbDepartmentUser departmentUser = await _repository.GetAsync(context.Message.UserId);

      if (departmentUser is not null)
      {
        await _repository.RemoveAsync(context.Message.UserId, context.Message.ModifiedBy);

        await _globalCache.RemoveAsync(departmentUser.DepartmentId);
      }
    }
  }
}
