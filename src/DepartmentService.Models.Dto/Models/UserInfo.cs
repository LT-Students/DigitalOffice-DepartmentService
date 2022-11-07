using System;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Models
{
  public record UserInfo
  {
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public Guid? ImageId { get; set; }
    public DepartmentUserInfo DepartmentUser { get; set; }
    public PositionInfo Position { get; set; }
  }
}
