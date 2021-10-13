using FluentValidation;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.DepartmentService.Validation.Interfaces;

namespace LT.DigitalOffice.DepartmentService.Validation.Department
{
  public class CreateDepartmentRequestValidator : AbstractValidator<CreateDepartmentRequest>, ICreateDepartmentRequestValidator
  {
    public CreateDepartmentRequestValidator()
    {
      When(request => request.Users != null, () =>
      {
        RuleForEach(request => request.Users)
          .NotEmpty().WithMessage("Users can not be empty.");
      });

      When(request => request.DirectorUserId != null, () =>
      {
        RuleFor(request => request.DirectorUserId)
          .NotEmpty().WithMessage("Director id can not be empty.");
      });

      RuleFor(request => request.Name)
        .NotEmpty().WithMessage("Department name can not be empty.")
        .MinimumLength(2).WithMessage("Department name is too short")
        .MaximumLength(100).WithMessage("Department name is too long.");

      When(request => request.Description != null, () =>
      {
        RuleFor(request => request.Description)
          .MaximumLength(1000).WithMessage("Department description is too long.");
      });
    }
  }
}
