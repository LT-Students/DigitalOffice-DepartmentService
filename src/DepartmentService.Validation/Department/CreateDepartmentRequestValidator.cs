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
      IDepartmentRepository departmentRepository,
      ICategoryRepository categoryRepository,
      ICreateUsersValidator usersValidator)
    {
      RuleFor(request => request.Name)
        .Must(n => n.Trim().Length > 1).WithMessage("Department name is too short.")
        .MaximumLength(300).WithMessage("Department name is too long.")
        .MustAsync(async (request, _) => !await departmentRepository.NameExistAsync(request))
        .WithMessage("The department name is already exists.");

      RuleFor(request => request.ShortName)
        .Must(n => n.Trim().Length > 1).WithMessage("Department short name is too short.")
        .MaximumLength(40).WithMessage("Department short name is too long.")
        .MustAsync(async (request, _) => !await departmentRepository.ShortNameExistAsync(request))
        .WithMessage("The department name is already exists.");

      When(request => request.Description is not null, () =>
      {
        RuleFor(request => request.Description)
          .MaximumLength(1000).WithMessage("Department description is too long.");
      });

      When(request => request.ParentId.HasValue, () =>
      {
        RuleFor(request => request.ParentId)
          .NotEmpty()
          .WithMessage("ParentId must not be empty.")
          .MustAsync(async (parentId, _) => await departmentRepository.ExistAsync(parentId.Value))
          .WithMessage("This department id doesn't exist.");
      });

      When(request => request.CategoryId.HasValue, () =>
      {
        RuleFor(request => request.CategoryId)
          .NotEmpty()
          .WithMessage("CategoryId must not be empty.")
          .MustAsync(async (categoryId, _) => await categoryRepository.IdExistAsync(categoryId.Value))
          .WithMessage("This category id doesn't exist.");
      });

      RuleFor(request => request.Users)
        .SetValidator(usersValidator);
    }
  }
}

