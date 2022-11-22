using FluentValidation;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department;
using LT.DigitalOffice.DepartmentService.Validation.Department.Interfaces;
using LT.DigitalOffice.DepartmentService.Validation.Department.Resources;
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
        .Must(n => n.Trim().Length > 1).WithMessage(CreateDepartmentRequestValidatorResource.NameTooShort)
        .MaximumLength(300).WithMessage(CreateDepartmentRequestValidatorResource.NameTooLong)
        .MustAsync(async (request, _) => !await departmentRepository.NameExistAsync(request))
        .WithMessage(CreateDepartmentRequestValidatorResource.ExistingName);

      RuleFor(request => request.ShortName)
        .Must(n => n.Trim().Length > 1).WithMessage(CreateDepartmentRequestValidatorResource.ShortNameTooShort)
        .MaximumLength(40).WithMessage(CreateDepartmentRequestValidatorResource.ShortNameTooLong)
        .MustAsync(async (request, _) => !await departmentRepository.ShortNameExistAsync(request))
        .WithMessage(CreateDepartmentRequestValidatorResource.ExistingShortName);

      When(request => request.Description is not null, () =>
      {
        RuleFor(request => request.Description)
          .MaximumLength(1000).WithMessage(CreateDepartmentRequestValidatorResource.DescriptionTooLong);
      });

      When(request => request.ParentId.HasValue, () =>
      {
        RuleFor(request => request.ParentId)
          .NotEmpty()
          .WithMessage(CreateDepartmentRequestValidatorResource.EmptyParentId)
          .MustAsync(async (parentId, _) => await departmentRepository.ExistAsync(parentId.Value))
          .WithMessage(CreateDepartmentRequestValidatorResource.NotExistingDepartment);
      });

      When(request => request.CategoryId.HasValue, () =>
      {
        RuleFor(request => request.CategoryId)
          .NotEmpty()
          .WithMessage(CreateDepartmentRequestValidatorResource.EmptyCategoryId)
          .MustAsync(async (categoryId, _) => await categoryRepository.IdExistAsync(categoryId.Value))
          .WithMessage(CreateDepartmentRequestValidatorResource.NotExistingCategory);
      });

      RuleFor(request => request.Users)
        .SetValidator(usersValidator);
    }
  }
}

