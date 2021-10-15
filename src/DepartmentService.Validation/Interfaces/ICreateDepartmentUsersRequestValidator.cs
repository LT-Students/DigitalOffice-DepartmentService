using FluentValidation;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.DepartmentService.Validation.Interfaces
{
  [AutoInject]
  public interface ICreateDepartmentUsersRequestValidator : IValidator<CreateDepartmentUsersRequest>
  {
  }
}
