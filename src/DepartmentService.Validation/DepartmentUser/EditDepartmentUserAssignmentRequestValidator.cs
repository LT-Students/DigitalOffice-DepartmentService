using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.DepartmentUser;
using LT.DigitalOffice.DepartmentService.Validation.DepartmentUser.Interfaces;

namespace LT.DigitalOffice.DepartmentService.Validation.DepartmentUser
{
  public class IEditDepartmentUserAssignmentRequestValidator
    : AbstractValidator<EditDepartmentUserAssignmentRequest>, IEditDepartmentUsersAssignmentRequestValidator
  {
    public IEditDepartmentUserAssignmentRequestValidator(
      IDepartmentUserRepository _repository)
    {
      List<DbDepartmentUser> dbDepartmentUsers = new();

      When(request => request.Assignment == DepartmentUserAssignment.Director, () =>
        RuleFor(request => request)
          .Must(request => request.UsersIds.Count < 2)
        .WithMessage("Only one user can be the department director."));

      RuleFor(request => request)
        .MustAsync(async (request, _) =>
        {
          dbDepartmentUsers = await _repository.GetAsync(request.UsersIds);
          return dbDepartmentUsers is not null && dbDepartmentUsers.Any();
        })
        .WithMessage("Department users was not found.")
        .Must(request => dbDepartmentUsers.FirstOrDefault(du => du.Assignment == (int)request.Assignment) is null)
        .WithMessage("Yser already has the assignment.")
        .Must(request => request.UsersIds.Distinct().Count() == dbDepartmentUsers.Count()
          && dbDepartmentUsers.Where(du => request.UsersIds.Contains(du.UserId)).Count() == dbDepartmentUsers.Count())
        .WithMessage("Some users do not exist.");
    }
  }
}
