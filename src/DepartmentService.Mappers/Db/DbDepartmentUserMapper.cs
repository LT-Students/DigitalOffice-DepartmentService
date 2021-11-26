using System;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.Kernel.Extensions;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.DepartmentService.Mappers.Db
{
  public class DbDepartmentUserMapper : IDbDepartmentUserMapper
  {
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DbDepartmentUserMapper(IHttpContextAccessor httpContextAccessor)
    {
      _httpContextAccessor = httpContextAccessor;
    }

    public DbDepartmentUser Map(CreateUserRequest request, Guid departmentId)
    {
      if (request == null)
      {
        return null;
      }

      return new DbDepartmentUser
      {
        Id = Guid.NewGuid(),
        UserId = request.UserId,
        DepartmentId = departmentId,
        IsActive = true,
        Role = (int)request.Role,
        CreatedBy = _httpContextAccessor.HttpContext.GetUserId(),
        CreatedAtUtc = DateTime.UtcNow
      };
    }

    public DbDepartmentUser Map(Guid userId, Guid departmentId, Guid? createdBy = null)
    {
      return new DbDepartmentUser()
      {
        Id = Guid.NewGuid(),
        UserId = userId,
        DepartmentId = departmentId,
        IsActive = true,
        Role = (int)DepartmentUserRole.Employee,
        CreatedBy = createdBy ?? _httpContextAccessor.HttpContext.GetUserId(),
        CreatedAtUtc = DateTime.UtcNow
      };
    }
  }
}
