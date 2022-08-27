using System;
using System.Collections.Generic;
using System.Linq;
using LT.DigitalOffice.DepartmentService.Broker.Helpers.Branch.Interfaces;

namespace LT.DigitalOffice.DepartmentService.Broker.Helpers.Branch
{
  public class DepartmentChildren : IDepartmentChildren
  {
    public void GetChildrenIds(in List<Tuple<Guid, string, string, Guid?>> listDepartments, Guid? idParent, List<Guid> archivedIds)
    {
      List<Guid> childrenIds = listDepartments.Where(ld => ld.Item4 == idParent).Select(ld => ld.Item1).ToList();
      archivedIds.AddRange(childrenIds);

      foreach (Guid childId in childrenIds)
      {
        GetChildrenIds(listDepartments, childId, archivedIds);
      }
    }
  }
}
