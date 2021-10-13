using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.DepartmentService.Validation.Interfaces;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Models.Broker.Common;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.DepartmentService.Validation
{
  public class AddDepartmentUsersRequestValidator : AbstractValidator<AddDepartmentUsersRequest>, IAddDepartmentUsersRequestValidator
  {
    private readonly IRequestClient<ICheckUsersExistence> _rcCheckUsersExistence;
    private readonly ILogger<AddDepartmentUsersRequestValidator> _logger;

    public AddDepartmentUsersRequestValidator(
      IRequestClient<ICheckUsersExistence> rcCheckUsersExistence,
      ILogger<AddDepartmentUsersRequestValidator> logger)
    {
      _rcCheckUsersExistence = rcCheckUsersExistence;
      _logger = logger;

      RuleFor(x => x.UsersIds)
        .Cascade(CascadeMode)
        .NotEmpty().WithMessage("Users ids must not be null.")
        .Must(x => x.Any()).WithMessage("Users ids must not be empty.")
        .Must(x => !x.Contains(Guid.Empty)).WithMessage("Users ids must not contains empty value.")
        .Must(x => x.Count() == x.Distinct().Count()).WithMessage("User cannot be added to the department twice.")
        .MustAsync(async (x, cancellation) => await CheckUserExistence(x)).WithMessage("Users ids contains invalid id.");
    }

    private async Task<bool> CheckUserExistence(List<Guid> usersIds)
    {
      try
      {
        Response<IOperationResult<ICheckUsersExistence>> response =
          await _rcCheckUsersExistence.GetResponse<IOperationResult<ICheckUsersExistence>>(
            ICheckUsersExistence.CreateObj(usersIds));

        if (response.Message.IsSuccess)
        {
          return usersIds.Count == response.Message.Body.UserIds.Count;
        }

        _logger.LogWarning("Can not find user Ids: {userIds}: " +
          $"{Environment.NewLine}{string.Join('\n', response.Message.Errors)}");
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, "Cannot check existing users withs this ids {userIds}");
      }

      return false;
    }
  }
}
