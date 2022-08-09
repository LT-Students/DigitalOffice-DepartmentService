using System.ComponentModel.DataAnnotations;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Category
{
  public class CreateCategoryRequest
  {
    [Required]
    public string Name { get; set; }
  }
}
