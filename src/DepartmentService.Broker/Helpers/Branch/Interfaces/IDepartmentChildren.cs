using System;
using System.Collections.Generic;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.DepartmentService.Broker.Helpers.Branch.Interfaces
{
  [AutoInject]
  public interface IDepartmentChildren
  {
    void GetChildrenIds(in List<Tuple<Guid, string, string, Guid?>> listDepartments, Guid? idParent, List<Guid> archivedIds);
  }
}
