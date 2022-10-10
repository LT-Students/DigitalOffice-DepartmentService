using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Enums;

namespace LT.DigitalOffice.DepartmentService.Broker.Helpers.Branch.Interfaces
{
  [AutoInject]
  public interface IDepartmentBranchHelper
  {
    List<Guid> GetChildrenIds(in List<Tuple<Guid, string, string, Guid?>> listDepartments, Guid parentDepartmentId);
    Task<DepartmentUserRole?> GetDepartmentUserRole(Guid userId, Guid departmentId);
  }
}
