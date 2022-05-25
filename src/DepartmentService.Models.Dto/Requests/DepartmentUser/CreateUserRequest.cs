using System;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Requests
{
  public record CreateUserRequest
  {
    public Guid UserId { get; set; }
    public DepartmentUserRole Role { get; set; }
    public DepartmentUserAssignment Assignment { get; set; }
  }
}
