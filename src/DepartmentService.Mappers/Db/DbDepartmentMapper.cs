using System;
using System.Linq;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.Kernel.Extensions;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.DepartmentService.Mappers.Db
{
  public class DbDepartmentMapper : IDbDepartmentMapper
  {
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDbDepartmentUserMapper _departmentUserMapper;

    public DbDepartmentMapper(
      IHttpContextAccessor httpContextAccessor,
      IDbDepartmentUserMapper departmentUserMapper)
    {
      _httpContextAccessor = httpContextAccessor;
      _departmentUserMapper = departmentUserMapper;
    }

    public DbDepartment Map(CreateDepartmentRequest request)
    {
      if (request == null)
      {
        return null;
      }

      Guid departmentId = Guid.NewGuid();

      return new DbDepartment
      {
        Id = departmentId,
        Name = request.Name,
        Description = request.Description,
        IsActive = true,
        CreatedBy = _httpContextAccessor.HttpContext.GetUserId(),
        CreatedAtUtc = DateTime.UtcNow,
        Users = request.Users?
          .Select(du => _departmentUserMapper.Map(du, departmentId))
          .ToList()
      };
    }
  }
}
