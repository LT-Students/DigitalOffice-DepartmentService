using System;
using System.Collections.Generic;
using System.Linq;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models
{
  public class DepartmentsTreeInfoMapper : IDepartmentsTreeInfoMapper
  {
    public List<DepartmentsTreeInfo> Map(List<Tuple<Guid, string, string, Guid?>> listDepartments, Guid? idParent)
    {
      List<DepartmentsTreeInfo> children = listDepartments.Where(p => p.Item4 == idParent).Select(x => new DepartmentsTreeInfo { Id = x.Item1, Name = x.Item2, CategoryName = x.Item3, ParentId = x.Item4 }).ToList();

      foreach (DepartmentsTreeInfo child in children)
      {
        child.Children = Map(listDepartments, child.Id);
      }

      return children;
    }
  }
}
