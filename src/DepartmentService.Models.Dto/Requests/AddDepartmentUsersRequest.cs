using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Requests
{
  public record AddDepartmentUsersRequest
  {
    public Guid DepartmentId { get; set; }
    public List<Guid> UsersIds { get; set; }
  }
}
