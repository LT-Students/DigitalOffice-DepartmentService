using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using LT.DigitalOffice.DepartmentService.Broker.Requests.Interfaces;
using LT.DigitalOffice.DepartmentService.Validation.Interfaces;
using LT.DigitalOffice.Models.Broker.Common;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.DepartmentService.Validation
{
  public class DepartmentUsersValidator : AbstractValidator<List<Guid>>, IDepartmentUsersValidator
  {
    private readonly IRequestClient<ICheckUsersExistence> _rcCheckUsersExistence;
    private readonly ILogger<DepartmentUsersValidator> _logger;

    public DepartmentUsersValidator(
      IRequestClient<ICheckUsersExistence> rcCheckUsersExistence,
      ILogger<DepartmentUsersValidator> logger)
    {
      _rcCheckUsersExistence = rcCheckUsersExistence;
      _logger = logger;
      IUserService _userservice = default;

      RuleForEach(users => users)
        .Cascade(CascadeMode.Stop)
        .NotEmpty().WithMessage("Users ids must not be empty.")
        .ChildRules(users =>
          RuleFor(users => users)
            .Must(ids => ids.Distinct().Count() == ids.Count())
            .WithMessage("User cannot be added to the deaprtment twice.")
            .MustAsync(async (ids, _) => ids.Count == (await _userservice.CheckUsersExistenceAsync(ids)).UserIds.Count)
            .WithMessage("Some users does not exist."));
    }
  }
}
