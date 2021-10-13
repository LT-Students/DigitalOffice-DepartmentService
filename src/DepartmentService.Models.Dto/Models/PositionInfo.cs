using System;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Models
{
  public record PositionInfo
  {
    public Guid Id { get; set; }
    public string Name { get; set; }
  }
}
