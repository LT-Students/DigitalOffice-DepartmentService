using System;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Requests
{
  public record EditDepartmentRequest
  {
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid DirectorId { get; set; }
    public bool IsActive { get; set; }
    public Guid? DepartmentId { get; set; }
  }
}
