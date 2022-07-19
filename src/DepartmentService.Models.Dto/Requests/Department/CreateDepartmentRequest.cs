using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department
{
  [Trim]
  public record CreateDepartmentRequest
  {
    [Required]
    public string Name { get; set; }
    [Required]
    public Guid ParentId { get; set; }
    [Required]
    public string ShortName { get; set; }
    public string Description { get; set; }
    [Required]
    public List<CreateUserRequest> Users { get; set; }
  }
}
