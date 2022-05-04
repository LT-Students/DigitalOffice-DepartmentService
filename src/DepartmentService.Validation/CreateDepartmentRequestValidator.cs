using System.Linq;
using FluentValidation;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.DepartmentService.Validation.Interfaces;

namespace LT.DigitalOffice.DepartmentService.Validation.Department
{
  public class CreateDepartmentRequestValidator : AbstractValidator<CreateDepartmentRequest>, ICreateDepartmentRequestValidator
  {

    public CreateDepartmentRequestValidator(
      IDepartmentRepository repository,
      IDepartmentUsersValidator departmentusersvalidator)
    {

      RuleFor(request => request.Name)
        .Cascade(CascadeMode.Stop)
        .NotEmpty().WithMessage("Department name can not be empty.")
        .Must(n => n.Trim().Length > 2).WithMessage("Department name is too short.")
        .MaximumLength(100).WithMessage("Department name is too long.")
        .MustAsync(async (request, _) => !await repository.NameExistAsync(request))
        .WithMessage("The department name is already exists.");

      When(request => request.Description != null, () =>
      {
        RuleFor(request => request.Description)
          .MaximumLength(1000).WithMessage("Department description is too long.");
      });

      RuleFor(request => request.Users)
        .NotNull()
        .WithMessage("Users should not be empty.")
        .ChildRules(dus =>
            dus.When(dus => dus.Any(), () =>
            {
                dus.RuleForEach(dus => dus)
                  .ChildRules(du =>
                      du.RuleFor(u => u.Role)
                        .IsInEnum().WithMessage("Wrong type of user role."));

                dus.RuleFor(dus => dus)
                  .Must(dus => dus.Where(u => u.Role == Models.Dto.Enums.DepartmentUserRole.Director).Count() < 2)
                  .WithMessage("Only one user can be the department director")
                  .ChildRules(dus =>
                      dus.RuleFor(dus => dus.Select(u => u.UserId).ToList())
                        .SetValidator(departmentusersvalidator));
            }));
    }
  }
}

