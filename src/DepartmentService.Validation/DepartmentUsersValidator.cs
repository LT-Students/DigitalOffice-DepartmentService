using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using LT.DigitalOffice.DepartmentService.Broker.Requests.Interfaces;
using LT.DigitalOffice.DepartmentService.Validation.Interfaces;

namespace LT.DigitalOffice.DepartmentService.Validation
{
  public class DepartmentUsersValidator : AbstractValidator<List<Guid>>, IDepartmentUsersValidator
  {
    public DepartmentUsersValidator(
      IUserService userservice)
    {
      RuleForEach(users => users)
        .Cascade(CascadeMode.Stop)
        .NotEmpty().WithMessage("Wrong type of user Id.")
        .ChildRules(users =>
          RuleFor(users => users)
            .Must(ids => ids.Distinct().Count() == ids.Count())
            .WithMessage("User cannot be added to the deaprtment twice.")
            .MustAsync(async (ids, _) => ids.Count == (await userservice.CheckUsersExistenceAsync(ids)).Count)
            .WithMessage("Some users does not exist."));
    }
  }
}
