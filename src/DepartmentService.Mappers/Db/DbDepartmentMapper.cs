using System;
using System.Linq;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.Kernel.Extensions;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.DepartmentService.Mappers.Db
{
  public class DbDepartmentMapper : IDbDepartmentMapper
  {
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DbDepartmentMapper(IHttpContextAccessor httpContextAccessor)
    {
      _httpContextAccessor = httpContextAccessor;
    }

    public DbDepartment Map(CreateDepartmentRequest request)
    {
      if (request == null)
      {
        return null;
      }

      Guid departmentId = Guid.NewGuid();
      Guid authorId = _httpContextAccessor.HttpContext.GetUserId();

      DbDepartment dbDepartment = new DbDepartment
      {
        Id = departmentId,
        Name = request.Name,
        Description = request.Description,
        IsActive = true,
        CreatedBy = authorId,
        CreatedAtUtc = DateTime.UtcNow,
        Users = request.Users?.Select(du => GetDbDepartmentUser(departmentId, du, authorId, DepartmentUserRole.Employee)).ToList() ?? new()
      };

      if (request.DirectorUserId.HasValue)
      {
        dbDepartment.Users.Add(GetDbDepartmentUser(departmentId, request.DirectorUserId.Value, authorId, DepartmentUserRole.Director));
      }

      return dbDepartment;
    }

    private DbDepartmentUser GetDbDepartmentUser(Guid departmentId, Guid userId, Guid authorid, DepartmentUserRole role)
    {
      return new DbDepartmentUser
      {
        Id = Guid.NewGuid(),
        DepartmentId = departmentId,
        UserId = userId,
        Role = (int)role,
        IsActive = true,
        CreatedAtUtc = DateTime.UtcNow,
        CreatedBy = authorid
      };
    }
  }
}
