using System;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.DepartmentUser;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;

namespace LT.DigitalOffice.DepartmentService.Business.User.Interfaces
{
  [AutoInject]
  public interface IEditDepartmentUsersAssigmentCommand
  {
    Task<OperationResultResponse<bool>> ExecuteAsync(Guid departmentId, EditDepartmentUserAssignmentRequest request);
  }
}
