using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Models.Broker.Models.Department;
using LT.DigitalOffice.Models.Broker.Requests.Department;
using LT.DigitalOffice.Models.Broker.Responses.Department;
using MassTransit;

namespace LT.DigitalOffice.DepartmentService.Broker.Consumers
{
  public class GetDepartmentsUsersConsumer : IConsumer<IGetDepartmentsUsersRequest>
  {
    private readonly IDepartmentUserRepository _repository;

    private async Task<object> FindUsersAsync(IGetDepartmentsUsersRequest request)
    {
      List<DbDepartmentUser> departmentsUsers = await _repository.GetAsync(request);

      return IGetDepartmentsUsersResponse.CreateObj(
        departmentsUsers.Select(du =>
          new DepartmentUserExtendedData(
            userId: du.UserId,
            departmentId: du.DepartmentId,
            isActive: du.IsActive))
        .ToList());
    }

    public GetDepartmentsUsersConsumer(
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
