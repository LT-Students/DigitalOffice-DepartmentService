using FluentValidation;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Category;
using LT.DigitalOffice.DepartmentService.Validation.Category.Interfaces;

namespace LT.DigitalOffice.DepartmentService.Validation.Category
{
  public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>, ICreateCategoryRequestValidator
  {
    public CreateCategoryRequestValidator(ICategoryRepository categoryRepository)
    {
      RuleFor(category => category.Name)
        .MustAsync(async (name, _) => !await categoryRepository.DoesAlreadyExistAsync(name))
        .WithMessage("Department category with this name already exists.");
    }
  }
}
