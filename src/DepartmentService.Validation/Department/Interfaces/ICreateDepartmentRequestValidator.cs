using FluentValidation;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.DepartmentService.Validation.Department.Interfaces
{
  [AutoInject]
  public interface ICreateDepartmentRequestValidator : IValidator<CreateDepartmentRequest>
  {
  }
}
