using System;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Models
{
  public record DepartmentUserInfo
  {
    public UserInfo User { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsActive { get; set; }
    public DepartmentUserRole Role { get; set; }
    public DepartmentUserAssignment Assignment { get; set; }
  }
}
