using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;

namespace LT.DigitalOffice.DepartmentService.Business.User.Interfaces
{
  [AutoInject]
  public interface ICreateDepartmentUsersCommand
  {
    Task<OperationResultResponse<bool>> ExecuteAsync(CreateDepartmentUsersRequest request);
  }
}
