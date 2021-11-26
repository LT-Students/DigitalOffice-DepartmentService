using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Requests
{
  public record CreateDepartmentUsersRequest
  {
    public Guid DepartmentId { get; set; }
    public List<CreateUserRequest> Users { get; set; }
  }
}
