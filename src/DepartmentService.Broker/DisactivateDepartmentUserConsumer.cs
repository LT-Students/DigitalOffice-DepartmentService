using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.Models.Broker.Common;
using MassTransit;

namespace LT.DigitalOffice.DepartmentService.Broker
{
  public class DisactivateDepartmentUserConsumer : IConsumer<IDisactivateUserRequest>
  {
    private readonly IDepartmentUserRepository _repository;

    public DisactivateDepartmentUserConsumer(
      IDepartmentUserRepository repository)
    {
      _repository = repository;
    }

    public async Task Consume(ConsumeContext<IDisactivateUserRequest> context)
    {
      await _repository.RemoveAsync(context.Message.UserId, context.Message.ModifiedBy);
    }
  }
}
