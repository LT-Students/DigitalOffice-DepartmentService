using System;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Requests
{
  public record EditDepartmentProjectRequest
  {
    public Guid ProjectId { get; set; }
    public Guid? DepartmentId { get; set; }
  }
}
