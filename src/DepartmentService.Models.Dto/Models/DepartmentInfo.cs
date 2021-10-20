using System;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Models
{
  public record DepartmentInfo
  {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int CountUsers { get; set; }
    public bool IsActive { get; set; }
    public UserInfo Director { get; set; }
  }
}
