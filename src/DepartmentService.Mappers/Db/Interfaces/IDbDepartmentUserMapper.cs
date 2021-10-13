using System;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces
{
  [AutoInject]
  public interface IDbDepartmentUserMapper
  {
    DbDepartmentUser Map(Guid userId, Guid departmentId, Guid modifiedBy);
  }
}
