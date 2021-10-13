using System;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;

namespace LT.DigitalOffice.DepartmentService.Mappers.Db
{
  public class DbDepartmentNewsMapper : IDbDepartmentNewsMapper
  {
    public DbDepartmentNews Map(Guid newsId, Guid departmentId, Guid modifiedBy)
    {
      return new DbDepartmentNews
      {
        Id = Guid.NewGuid(),
        NewsId = newsId,
        DepartmentId = departmentId,
        IsActive = true,
        CreatedBy = modifiedBy,
        CreatedAtUtc = DateTime.UtcNow
      };
    }
  }
}
