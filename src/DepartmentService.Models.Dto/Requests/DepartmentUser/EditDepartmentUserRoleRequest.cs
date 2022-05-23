using System;
using System.Collections.Generic;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Requests.DepartmentUser
{
  public record EditDepartmentUserAssignmentRequest
  {
    public DepartmentUserAssignment Assignment { get; set; }
    public List<Guid> UsersIds { get; set; }
  }
}
