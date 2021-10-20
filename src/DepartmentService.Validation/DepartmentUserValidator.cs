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
  public class DepartmentUserValidator : AbstractValidator<CreateUserRequest>, IDepartmentUserValidator
  {
    private readonly IRequestClient<ICheckUsersExistence> _rcCheckUsersExistence;
    private readonly ILogger<DepartmentUserValidator> _logger;

    public DepartmentUserValidator(
      IRequestClient<ICheckUsersExistence> rcCheckUsersExistence,
      ILogger<DepartmentUserValidator> logger)
    {
      _rcCheckUsersExistence = rcCheckUsersExistence;
      _logger = logger;

      RuleFor(pu => pu.UserId)
        .NotEmpty()
        .WithMessage("Not specified user id.")
        .MustAsync(async (x, cancellation) => await CheckUsersExistenceAsync(new() { x }))
        .WithMessage("Users ids contains invalid id.");

      RuleFor(pu => pu.Role)
        .IsInEnum();
    }
    private async Task<bool> CheckUsersExistenceAsync(List<Guid> usersIds)
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
