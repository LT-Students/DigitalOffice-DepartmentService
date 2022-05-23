using FluentValidation;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.DepartmentService.Validation.DepartmentUser.Interfaces;

namespace LT.DigitalOffice.DepartmentService.Validation.DepartmentUser
{
  public class CreateDepartmentUsersValidator : AbstractValidator<CreateDepartmentUsersRequest>, ICreateDepartmentUsersValidator
  {
    public CreateDepartmentUsersValidator(
      IDepartmentRepository repository,
      ICreateUsersValidator usersValidator)
    {
      RuleFor(request => request.DepartmentId)
        .NotEmpty().WithMessage("Departmment id can not be empty.")
        .MustAsync(async (i, _) => await repository.ExistAsync(i))
        .WithMessage("The department id does not exist.");

      RuleFor(request => request.Users)
        .SetValidator(usersValidator);
    }
  }
}
