using System;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;

namespace LT.DigitalOffice.DepartmentService.Mappers.Db
{
  public class DbDepartmentUserMapper : IDbDepartmentUserMapper
  {
    public DbDepartmentUser Map(Guid userId, Guid departmentId, Guid createdBy)
    {
      return new DbDepartmentUser
      {
        Id = Guid.NewGuid(),
        UserId = userId,
        DepartmentId = departmentId,
        IsActive = true,
        CreatedBy = createdBy,
        CreatedAtUtc = DateTime.UtcNow
      };
    }
  }
}
