using System;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.Kernel.Extensions;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.DepartmentService.Mappers.Db
{
  public class DbDepartmentProjectMapper : IDbDepartmentProjectMapper
  {
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DbDepartmentProjectMapper(IHttpContextAccessor httpContextAccessor)
    {
      _httpContextAccessor = httpContextAccessor;
    }

    public DbDepartmentProject Map(Guid projectId, Guid departmentId, Guid createdBy)
    {
      return new DbDepartmentProject
      {
        Id = Guid.NewGuid(),
        ProjectId = projectId,
        DepartmentId = departmentId,
        IsActive = true,
        CreatedBy = createdBy,
        CreatedAtUtc = DateTime.UtcNow
      };
    }

    public DbDepartmentProject Map(Guid projectId, Guid departmentId)
    {
      return new DbDepartmentProject
      {
        Id = Guid.NewGuid(),
        ProjectId = projectId,
        DepartmentId = departmentId,
        IsActive = true,
        CreatedBy = _httpContextAccessor.HttpContext.GetUserId(),
        CreatedAtUtc = DateTime.UtcNow
      };
    }
  }
}
