using System;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.Models.Broker.Enums;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Models
{
  public record DepartmentUserInfo
  {
    public Guid UserId { get; set; }
    public DepartmentUserRole Role { get; set; }
    public DepartmentUserAssignment Assignment { get; set; }
  }
}
