using System;
using System.Collections.Generic;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Responses
{
  public record DepartmentResponse
  {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string ShortName { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public Guid? ParentId { get; set; }
    public CategoryInfo Category { get; set; }
    public IEnumerable<DepartmentUserInfo> Users { get; set; }
  }
}
