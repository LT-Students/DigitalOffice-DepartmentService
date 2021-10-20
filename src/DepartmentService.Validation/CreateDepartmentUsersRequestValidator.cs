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

namespace LT.DigitalOffice.DepartmentService.Validation
{
  public class CreateDepartmentUsersRequestValidator : AbstractValidator<CreateDepartmentUsersRequest>, ICreateDepartmentUsersRequestValidator
  {
    private readonly IDepartmentUserValidator _departmentUserValidator;
    private readonly IDepartmentRepository _departmentRepository;

    public CreateDepartmentUsersRequestValidator(
      IDepartmentUserValidator departmrntUserValidator,
      IDepartmentRepository departmentRepository)
    {
      _departmentRepository = departmentRepository;
      _departmentUserValidator = departmrntUserValidator;


      RuleFor(departmentUser => departmentUser.DepartmentId)
        .Cascade(CascadeMode.Stop)
        .NotEmpty()
        .WithMessage("Request must have a project Id")
        .MustAsync(async (x, _) => await _departmentRepository.DoesExistAsync(x))
        .WithMessage("This project id does not exist")
        .DependentRules(() =>
        {
          RuleFor(projectUser => projectUser.Users)
            .Must(user => user != null && user.Any())
            .WithMessage("The request must contain users");

          RuleForEach(projectUser => projectUser.Users)
            .SetValidator(_departmentUserValidator);
        });
    }
  }
}
