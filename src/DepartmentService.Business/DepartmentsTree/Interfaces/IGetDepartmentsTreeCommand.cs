using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department.Filters;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;

namespace LT.DigitalOffice.DepartmentService.Business.DepartmentsTree.Interfaces
{
  [AutoInject]
  public interface IGetDepartmentsTreeCommand
  {
    Task<OperationResultResponse<List<DepartmentsTreeInfo>>> ExecuteAsync(FindDepartmentFilter filter);
  }
}
