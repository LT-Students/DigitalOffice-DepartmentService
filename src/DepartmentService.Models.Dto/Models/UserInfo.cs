using System;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Models
{
  public record UserInfo
  {
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public DateTime? CreatedAtUtc { get; set; }
    public DateTime? LeftAtUtc { get; set; }
    public bool IsActive { get; set; }
    public DepartmentUserRole Role { get; set; }
    public ImageInfo AvatarImage { get; set; }
    public PositionInfo Position { get; set; }
  }
}
