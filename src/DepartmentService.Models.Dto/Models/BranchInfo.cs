using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Models
{
  public class BranchInfo
  {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid? ParentId { get; set; }
    public List<BranchInfo> Children { get; set; }
  }
}
