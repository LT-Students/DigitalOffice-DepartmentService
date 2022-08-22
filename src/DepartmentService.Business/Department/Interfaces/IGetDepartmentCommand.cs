using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department.Filters;
using LT.DigitalOffice.DepartmentService.Models.Dto.Responses;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;

namespace LT.DigitalOffice.DepartmentService.Business.Department.Interfaces
{
  [AutoInject]
  public interface IGetDepartmentCommand
  {
    Task<OperationResultResponse<DepartmentResponse>> ExecuteAsync(GetDepartmentFilter filter);
  }
}
