using FluentValidation;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Category;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.DepartmentService.Validation.Category.Interfaces
{
  [AutoInject]
  public interface ICreateCategoryRequestValidator : IValidator<CreateCategoryRequest>
  {
  }
}
