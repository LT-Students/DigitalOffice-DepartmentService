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
    private readonly ILogger<CreateDepartmentRequestValidator> _logger;

    public CreateDepartmentRequestValidator(
      IDepartmentRepository repository,
      IRequestClient<ICheckUsersExistence> rcCheckUsersExistence,
      ILogger<CreateDepartmentRequestValidator> logger)
    {
      _repository = repository;
      _rcCheckUsersExistence = rcCheckUsersExistence;
      _logger = logger;

      RuleFor(request => request.Name)
        .Cascade(CascadeMode.Stop)
        .NotEmpty().WithMessage("Department name can not be empty.")
        .Must(n => n.Trim().Length > 2).WithMessage("Department name is too short.")
        .MaximumLength(100).WithMessage("Department name is too long.")
        .MustAsync(async (request, _) => await _repository.NameExistAsync(request))
        .WithMessage("The department name is already exists.");

      When(request => request.Description != null, () =>
      {
        RuleFor(request => request.Description)
          .MaximumLength(1000).WithMessage("Department description is too long.");
      });

      When(department => department.Users != null && department.Users.Any(), () =>
      {
        RuleFor(department => department.Users)
          .Cascade(CascadeMode.Stop)
          .ChildRules(d =>
            RuleForEach(department => department.Users)
              .ChildRules(u =>
              {
                u.RuleFor(u => u.UserId)
                  .NotEmpty().WithMessage("Wrong type of user Id.");

                u.RuleFor(u => u.Role)
                  .IsInEnum().WithMessage("Wrong type of user role.");
              }))
          .Must(d => d.Select(du => du.UserId).Distinct().Count() == d.Count())
          .WithMessage("User cannot be added to the deaprtment twice.")
          .Must(d => d.Where(du => du.Role == Models.Dto.Enums.DepartmentUserRole.Director).Count() < 2)
          .WithMessage("Only one user can be the department director")
          .MustAsync(async (d, _) => await CheckUsersExistenceAsync(d.Select(du => du.UserId).ToList()))
          .WithMessage("Some users does not exist.");
      });
    }

    private async Task<bool> CheckUsersExistenceAsync(List<Guid> usersIds)
    {
      if (usersIds == null || !usersIds.Any())
      {
        return true;
      }

      try
      {
        Response<IOperationResult<ICheckUsersExistence>> response =
          await _rcCheckUsersExistence.GetResponse<IOperationResult<ICheckUsersExistence>>(
            ICheckUsersExistence.CreateObj(usersIds));

        if (response.Message.IsSuccess)
        {
          return response.Message.Body.UserIds.Count() == usersIds.Count();
        }

        _logger.LogWarning(
          "Error while checking users existence Ids: {UsersIds}.\nErrors: {Errors}.",
          string.Join(", ", usersIds),
          string.Join('\n', response.Message.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(
          exc,
          "Can not check users existence Ids: {UsersIds}.",
          string.Join(", ", usersIds));
      }

      return false;
    }
  }
}
