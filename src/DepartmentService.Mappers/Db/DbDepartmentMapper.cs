using System;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department;
using LT.DigitalOffice.Kernel.Extensions;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.DepartmentService.Mappers.Db
{
  public class DbDepartmentMapper : IDbDepartmentMapper
  {
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DbDepartmentMapper(
      IHttpContextAccessor httpContextAccessor)
    {
      _httpContextAccessor = httpContextAccessor;
    }

    public DbDepartment Map(CreateDepartmentRequest request)
    {
      return request is null
        ? null
        : new DbDepartment
        {
          Id = Guid.NewGuid(),
          Name = request.Name,
          ShortName = request.ShortName,
          Description = request.Description,
          IsActive = true,
          CreatedBy = _httpContextAccessor.HttpContext.GetUserId(),
          CreatedAtUtc = DateTime.UtcNow
        };
    }
  }
}
