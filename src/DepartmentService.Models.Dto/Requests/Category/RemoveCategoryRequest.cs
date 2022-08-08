using System;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Category
{
  public record RemoveCategoryRequest
  {
    public Guid Id { get; set; }
  }
}
