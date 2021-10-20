using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.DepartmentService.Validation.Interfaces;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Models.Broker.Common;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.DepartmentService.Validation.Department
{
  public class CreateDepartmentRequestValidator : AbstractValidator<CreateDepartmentRequest>, ICreateDepartmentRequestValidator
  {
    private readonly IDepartmentRepository _repository;
    private readonly IRequestClient<ICheckUsersExistence> _rcCheckUsersExistence;
    private readonly ILogger<CreateDepartmentUsersRequestValidator> _logger;

    public CreateDepartmentRequestValidator(
      IDepartmentRepository repository,
      IRequestClient<ICheckUsersExistence> rcCheckUsersExistence,
      ILogger<CreateDepartmentUsersRequestValidator> logger)
    {
      _repository = repository;
      _rcCheckUsersExistence = rcCheckUsersExistence;
      _logger = logger;

      When(department => department.Users != null && department.Users.Any(), () =>
      {
        RuleForEach(department => department.Users)
          .ChildRules(user =>
          {
            user.RuleFor(user => user.UserId)
              .NotEmpty().WithMessage("Wrong type of user Id.");

            user.RuleFor(user => user.Role)
              .IsInEnum();
          });

        RuleFor(department => department.Users)
          .Cascade(CascadeMode.Stop)
          .Must(d => d.Select(du => du.UserId).Distinct().Count() == d.Count())
          .WithMessage("User cannot be added to the deaprtment twice.")
          .MustAsync(async (du, cancellation) => await CheckUsersExistenceAsync(du.Select(u => u.UserId).ToList()))
          .WithMessage("Some users does not exist.");
      });

      When(request => request.DirectorUserId != null, () =>
      {
        RuleFor(request => request.DirectorUserId)
          .NotEmpty().WithMessage("Director id can not be empty.");
      });

      RuleFor(request => request.Name)
        .NotEmpty().WithMessage("Department name can not be empty.")
        .Must(n => n.Trim().Length > 2).WithMessage("Department name is too short")
        .MaximumLength(100).WithMessage("Department name is too long.")
        .MustAsync(async (request, collection) => await _repository.DoesNameExistAsync(request))
        .WithMessage("This department name is already exists.");

      When(request => request.Description != null, () =>
      {
        RuleFor(request => request.Description)
          .MaximumLength(1000).WithMessage("Department description is too long.");
      });
    }

    private async Task<bool> CheckUsersExistenceAsync(List<Guid> usersIds)
    {
      if (!usersIds.Any())
      {
        return true;
      }

      string logMessage = "Cannot check existing users withs this ids {userIds}";

      try
      {
        Response<IOperationResult<ICheckUsersExistence>> response =
          await _rcCheckUsersExistence.GetResponse<IOperationResult<ICheckUsersExistence>>(
            ICheckUsersExistence.CreateObj(usersIds));

        if (response.Message.IsSuccess)
        {
          return response.Message.Body.UserIds.Count() == usersIds.Count();
        }

        _logger.LogWarning($"Can not find with this Ids: {usersIds}: {Environment.NewLine}{string.Join('\n', response.Message.Errors)}");
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, logMessage);
      }

      return false;
    }
  }
}
