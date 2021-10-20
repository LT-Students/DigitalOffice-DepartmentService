using System;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;

namespace LT.DigitalOffice.DepartmentService.Business.Department.Interfaces
{
  [AutoInject]
  public interface ICreateDepartmentCommand
  {
    Task<OperationResultResponse<Guid?>> ExecuteAsync(CreateDepartmentRequest request);
  }
}
