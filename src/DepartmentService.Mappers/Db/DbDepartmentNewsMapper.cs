using System;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.Kernel.Extensions;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.DepartmentService.Mappers.Db
{
  public class DbDepartmentNewsMapper : IDbDepartmentNewsMapper
  {
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DbDepartmentNewsMapper(IHttpContextAccessor httpContextAccessor)
    {
      _httpContextAccessor = httpContextAccessor;
    }
    public DbDepartmentNews Map(Guid newsId, Guid departmentId)
    {
      return new DbDepartmentNews
      {
        Id = Guid.NewGuid(),
        NewsId = newsId,
        DepartmentId = departmentId,
        IsActive = true,
        CreatedBy = _httpContextAccessor.HttpContext.GetUserId(),
        CreatedAtUtc = DateTime.UtcNow
      };
    }
  }
}
