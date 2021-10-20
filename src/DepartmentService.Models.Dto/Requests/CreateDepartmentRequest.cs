using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Requests
{
  public record CreateDepartmentRequest
  {
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid? DirectorUserId { get; set; }
    public List<CreateUserRequest> Users { get; set; }
  }
}
