using System.Collections.Generic;
using FluentValidation;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.DepartmentService.Validation.DepartmentUser.Interfaces
{
  [AutoInject]
  public interface ICreateUsersValidator : IValidator<List<CreateUserRequest>>
  {
  }
}
