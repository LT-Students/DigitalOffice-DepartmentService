using System.Threading;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department.Filters;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;

namespace LT.DigitalOffice.DepartmentService.Business.Department.Interfaces
{
  [AutoInject]
  public interface IFindDepartmentsCommand
  {
    Task<FindResultResponse<DepartmentInfo>> ExecuteAsync(FindDepartmentFilter filter, CancellationToken cancellationToken = default);
  }
}
