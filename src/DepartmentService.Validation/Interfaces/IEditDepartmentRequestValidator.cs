using FluentValidation;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.Kernel.Attributes;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.DepartmentService.Validation.Interfaces
{
  [AutoInject]
  public interface IEditDepartmentRequestValidator : IValidator<JsonPatchDocument<EditDepartmentRequest>>
  {
  }
}
