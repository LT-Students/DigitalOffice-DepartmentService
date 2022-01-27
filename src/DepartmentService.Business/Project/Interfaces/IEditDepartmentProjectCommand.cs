using System;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;

namespace LT.DigitalOffice.DepartmentService.Business.Project.Interfaces
{
  [AutoInject]
  public interface IEditDepartmentProjectCommand
  {
    Task<OperationResultResponse<Guid?>> ExecuteAsync(EditDepartmentProjectRequest request);
  }
}
