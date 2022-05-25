using System;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;

namespace LT.DigitalOffice.DepartmentService.Mappers.Db
{
  public class DbDepartmentProjectMapper : IDbDepartmentProjectMapper
  {
    public DbDepartmentProject Map(Guid projectId, Guid departmentId, Guid createdBy)
    {
      return new DbDepartmentProject
      {
        Id = Guid.NewGuid(),
        ProjectId = projectId,
        DepartmentId = departmentId,
        IsActive = true,
        CreatedBy = createdBy
      };
    }
  }
}
