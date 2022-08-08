using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Models
{
  public class DepartmentsTreeInfo
  {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid? ParentId { get; set; }
    public string CategoryName { get; set; }
    public List<DepartmentsTreeInfo> Children { get; set; }
  }
}
