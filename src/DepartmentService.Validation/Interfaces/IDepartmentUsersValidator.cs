using System;
using System.Collections.Generic;
using FluentValidation;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.DepartmentService.Validation.Interfaces
{
  [AutoInject]
  public interface IDepartmentUsersValidator : IValidator<List<Guid>>
  {
  }
}
