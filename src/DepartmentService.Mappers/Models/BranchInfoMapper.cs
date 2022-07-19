using System;
using System.Collections.Generic;
using System.Linq;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models
{
  public class BranchInfoMapper : IBranchInfoMapper
  {
    public List<BranchInfo> Map(List<Tuple<Guid, string, Guid?>> listDepartments, Guid? idParent)
    {
      List<BranchInfo> children = listDepartments.Where(p => p.Item3 == idParent).Select(x => new BranchInfo { Id = x.Item1, Name = x.Item2, ParentId = x.Item3 }).ToList();

      foreach (BranchInfo child in children)
      {
        child.Children = Map(listDepartments, child.Id);
      }

      return children;
    }
  }
}
