using FluentValidation;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department;
using LT.DigitalOffice.DepartmentService.Validation.Department.Interfaces;
using LT.DigitalOffice.DepartmentService.Validation.DepartmentUser.Interfaces;

namespace LT.DigitalOffice.DepartmentService.Validation.Department
{
  public class CreateDepartmentRequestValidator : AbstractValidator<CreateDepartmentRequest>, ICreateDepartmentRequestValidator
  {
    public CreateDepartmentRequestValidator(
      IDepartmentRepository repository,
      ICreateUsersValidator usersValidator)
    {
      RuleFor(request => request.Name)
        .Must(n => n.Trim().Length > 2).WithMessage("Department name is too short.")
        .MaximumLength(300).WithMessage("Department name is too long.")
        .MustAsync(async (request, _) => !await repository.NameExistAsync(request))
        .WithMessage("The department name is already exists.");

      RuleFor(request => request.ShortName)
        .Must(n => n.Trim().Length > 2).WithMessage("Department short name is too short.")
        .MaximumLength(40).WithMessage("Department short name is too long.")
        .MustAsync(async (request, _) => !await repository.ShortNameExistAsync(request))
        .WithMessage("The department name is already exists.");

      When(request => request.Description is not null, () =>
      {
        RuleFor(request => request.Description)
          .MaximumLength(1000).WithMessage("Department description is too long.");
      });

      RuleFor(request => request.Users)
        .SetValidator(usersValidator);
    }
  }
}

