using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Models.Broker.Requests.Department;
using LT.DigitalOffice.Models.Broker.Responses.Department;
using MassTransit;

namespace LT.DigitalOffice.DepartmentService.Broker.Consumers
{
  public class GetDepartmentUsersConsumer : IConsumer<IGetDepartmentsUsersRequest>
  {
    private readonly IDepartmentUserRepository _repository;

    private async Task<object> FindUsersAsync(IGetDepartmentsUsersRequest request)
    {
      //(List<Guid> userIds, int totalCount) = await _repository.GetAsync(request);

      return null;//IGetDepartmentsUsersResponse.CreateObj(userIds, totalCount);
    }

    public GetDepartmentUsersConsumer(
      IDepartmentUserRepository repository)
    {
      _repository = repository;
    }

    public async Task Consume(ConsumeContext<IGetDepartmentsUsersRequest> context)
    {
      object result = OperationResultWrapper.CreateResponse(FindUsersAsync, context.Message);

      await context.RespondAsync<IOperationResult<IGetDepartmentsUsersResponse>>(result);
    }
  }
}
