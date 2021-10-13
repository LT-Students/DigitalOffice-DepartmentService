using System;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.DepartmentService.Business.Interfaces
{
  [AutoInject]
  public interface IEditDepartmentCommand
  {
    Task<OperationResultResponse<bool>> ExecuteAsync(Guid departmentId, JsonPatchDocument<EditDepartmentRequest> request);
  }
}
