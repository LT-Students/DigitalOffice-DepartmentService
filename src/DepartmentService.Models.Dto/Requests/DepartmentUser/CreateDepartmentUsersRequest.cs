using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Requests
{
  public record CreateDepartmentUsersRequest
  {
    public Guid DepartmentId { get; set; }
    [Required]
    public List<CreateUserRequest> Users { get; set; }
  }
}
