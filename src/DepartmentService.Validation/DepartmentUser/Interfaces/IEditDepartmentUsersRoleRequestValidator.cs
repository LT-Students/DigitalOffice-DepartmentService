using FluentValidation;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.DepartmentUser;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.DepartmentService.Validation.DepartmentUser.Interfaces
{
  [AutoInject]
  public interface IEditDepartmentUsersRoleRequestValidator
    : IValidator<EditDepartmentUserRoleRequest>
  {
  }
}
