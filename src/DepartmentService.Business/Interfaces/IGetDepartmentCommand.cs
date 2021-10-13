using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Filters;
using LT.DigitalOffice.DepartmentService.Models.Dto.Responses;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;

namespace LT.DigitalOffice.DepartmentService.Business.Interfaces
{
  [AutoInject]
  public interface IGetDepartmentCommand
  {
    Task<OperationResultResponse<DepartmentResponse>> ExecuteAsync(GetDepartmentFilter filter);
  }
}
