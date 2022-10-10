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
      //because dictionary can't have null as key, empty guid is used here for elements, whose parentId is null
      Dictionary<Guid, List<DepartmentsTreeInfo>> childrenDictionary = listDepartments.GroupBy(ld => ld.Item4 ?? Guid.Empty)
        .ToDictionary(ld => ld.Key, ld => ld.Select(x => new DepartmentsTreeInfo { Id = x.Item1, Name = x.Item2, CategoryName = x.Item3, ParentId = x.Item4 }).ToList());

      if (!childrenDictionary.TryGetValue(idParent ?? Guid.Empty, out var treeInfo))
      {
        return Enumerable.Empty<DepartmentsTreeInfo>().ToList();
      }

      Stack<DepartmentsTreeInfo> childrenStack = new(treeInfo);

      while (childrenStack.Any())
      {
        DepartmentsTreeInfo currentParent = childrenStack.Pop();

        if (childrenDictionary.TryGetValue(currentParent.Id, out var currentChildren))
        {
          currentParent.Children = currentChildren;

          foreach (var child in currentChildren)
          {
            childrenStack.Push(child);
          }
        }
      }

      return treeInfo;
    }
  }
}
