using System;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces
{
  [AutoInject]
  public interface IDbDepartmentNewsMapper
  {
    DbDepartmentNews Map(Guid newsId, Guid departmentId, Guid modifiedBy);
  }
}
