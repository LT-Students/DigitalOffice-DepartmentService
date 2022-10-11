using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Broker.Helpers.Branch.Interfaces;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Models.Broker.Requests.Department;
using LT.DigitalOffice.Models.Broker.Responses.Department;
using MassTransit;

namespace LT.DigitalOffice.DepartmentService.Broker.Consumers
{
  public class GetDepartmentUserRoleConsumer : IConsumer<IGetDepartmentUserRoleRequest>
  {
    private readonly IDepartmentBranchHelper _departmentBranchHelper;

    private async Task<object> CheckUserRoleAsync(IGetDepartmentUserRoleRequest request)
    {
      return IGetDepartmentUserRoleResponse.CreateObj(
        await _departmentBranchHelper.GetDepartmentUserRole(userId: request.UserId, departmentId: request.DepartmentId));
    }

    public GetDepartmentUserRoleConsumer(
      IDepartmentBranchHelper departmentBranchHelper)
    {
      _departmentBranchHelper = departmentBranchHelper;
    }

    public async Task Consume(ConsumeContext<IGetDepartmentUserRoleRequest> context)
    {
      object response = OperationResultWrapper.CreateResponse(CheckUserRoleAsync, context.Message);

      await context.RespondAsync<IOperationResult<IGetDepartmentUserRoleResponse>>(response);
    }
  }
}
