using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;

namespace LT.DigitalOffice.DepartmentService.Business.Department.Interfaces
{
  [AutoInject]
  public interface IGetBranchCommand
  {
    Task<OperationResultResponse<List<BranchInfo>>> ExecuteAsync();
  }
}
