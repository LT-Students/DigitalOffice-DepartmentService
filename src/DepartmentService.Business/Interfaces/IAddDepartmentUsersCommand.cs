using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;

namespace LT.DigitalOffice.DepartmentService.Business.Interfaces
{
  [AutoInject]
  public interface IAddDepartmentUsersCommand
  {
    Task<OperationResultResponse<bool>> ExecuteAsync(AddDepartmentUsersRequest request);
  }
}
