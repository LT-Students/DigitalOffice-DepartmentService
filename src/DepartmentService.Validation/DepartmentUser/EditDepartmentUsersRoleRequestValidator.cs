using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.DepartmentUser;
using LT.DigitalOffice.DepartmentService.Validation.DepartmentUser.Interfaces;

namespace LT.DigitalOffice.DepartmentService.Validation.DepartmentUser
{
  public class EditDepartmentUsersRoleRequestValidator
    : AbstractValidator<EditDepartmentUserRoleRequest>, IEditDepartmentUsersRoleRequestValidator
  {
    public EditDepartmentUsersRoleRequestValidator(
      IDepartmentUserRepository _repository)
    {
      List<DbDepartmentUser> dbDepartmentUsers = new();
      RuleFor(request => request)
        .MustAsync(async (request, _) =>
        {
          dbDepartmentUsers = await _repository.GetAsync(request.UsersIds);
          return dbDepartmentUsers is not null && dbDepartmentUsers.Any();
        })
        .WithMessage("Department users was not found.")
        .Must(request => dbDepartmentUsers.FirstOrDefault(du => du.Role == (int)request.Role) is null)
        .WithMessage("User already has the role.")
        .Must(request => request.UsersIds.Distinct().Count() == dbDepartmentUsers.Count()
          && dbDepartmentUsers.Where(du => request.UsersIds.Contains(du.UserId)).Count() == dbDepartmentUsers.Count())
        .WithMessage("Some users do not exist.");
    }
  }
}
