using System.Linq;
using FluentValidation;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.DepartmentService.Validation.Interfaces;

namespace LT.DigitalOffice.DepartmentService.Validation.Department
{
  public class CreateDepartmentRequestValidator : AbstractValidator<CreateDepartmentRequest>, ICreateDepartmentRequestValidator
  {
    private readonly IDepartmentRepository _repository;

    public CreateDepartmentRequestValidator(
      IDepartmentRepository repository,
      IDepartmentUsersValidator departmentusersvalidator)
    {
      _repository = repository;

      RuleFor(request => request.Name)
        .Cascade(CascadeMode.Stop)
        .NotEmpty().WithMessage("Department name can not be empty.")
        .Must(n => n.Trim().Length > 2).WithMessage("Department name is too short.")
        .MaximumLength(100).WithMessage("Department name is too long.")
        .MustAsync(async (request, _) => !await _repository.NameExistAsync(request))
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
                u.RuleFor(u => u.Role)
                  .IsInEnum().WithMessage("Wrong type of user role.");
              }))
          .Must(d => d.Where(du => du.Role == Models.Dto.Enums.DepartmentUserRole.Director).Count() < 2)
          .WithMessage("Only one user can be the department director")
          .ChildRules(d =>
             RuleFor(d => d.Users.Select(user => user.UserId).ToList())
             .SetValidator(departmentusersvalidator));
      });
    }
  }
}

