using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department
{
  public record CreateDepartmentRequest
  {
    [Required]
    public string Name { get; set; }
    public string Description { get; set; }
    [Required]
    public List<CreateUserRequest> Users { get; set; }
  }
}
