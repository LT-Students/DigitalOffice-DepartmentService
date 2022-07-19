using System;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department
{
  public record EditDepartmentRequest
  {
    public string Name { get; set; }
    public Guid ParentId { get; set; }
    public string ShortName { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
  }
}
