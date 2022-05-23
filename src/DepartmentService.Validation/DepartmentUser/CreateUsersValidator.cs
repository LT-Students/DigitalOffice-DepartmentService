using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using LT.DigitalOffice.DepartmentService.Broker.Requests.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.DepartmentService.Validation.DepartmentUser.Interfaces;

namespace LT.DigitalOffice.DepartmentService.Validation.DepartmentUser
{
  public class CreateUsersValidator : AbstractValidator<List<CreateUserRequest>>, ICreateUsersValidator
  {
    public CreateUsersValidator(
      IUserService userService)
    {
      RuleFor(request => request)
        .Must(request => request.Where(u => u.Assignment == DepartmentUserAssignment.Director).Count() < 2)
        .WithMessage("Only one user can be the department director.")
        .MustAsync(async (request, _) =>
        {
          List<Guid> usersIds = request.Select(r => r.UserId).ToList();
          return usersIds.Distinct().Count() == usersIds.Count()
            && (await userService.CheckUsersExistenceAsync(usersIds)).Count == usersIds.Count;
        })
        .WithMessage("Some users does not exist.");

      RuleForEach(request => request)
        .ChildRules(u =>
          u.RuleFor(u => u.Role)
            .IsInEnum()
            .WithMessage("Wrong type of user role."));

      RuleForEach(request => request)
        .ChildRules(u =>
          u.RuleFor(u => u.Assignment)
            .IsInEnum()
            .WithMessage("Wrong type of user role."));
    }
  }
}
