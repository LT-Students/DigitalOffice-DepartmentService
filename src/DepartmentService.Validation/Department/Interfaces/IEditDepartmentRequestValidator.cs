using FluentValidation;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department;
using LT.DigitalOffice.Kernel.Attributes;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.DepartmentService.Validation.Department.Interfaces
{
  [AutoInject]
  public interface IEditDepartmentRequestValidator : IValidator<JsonPatchDocument<EditDepartmentRequest>>
  {
  }
}
