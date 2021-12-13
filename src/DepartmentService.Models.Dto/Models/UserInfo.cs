using System;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Models
{
  public class UserInfo
  {
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public ImageInfo Avatar { get; set; }
  }
}
