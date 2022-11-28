using System;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Publishing.Subscriber.Department;
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
      return request is null
        ? default
        : new DbDepartmentUser
        {
          Id = Guid.NewGuid(),
          UserId = request.UserId,
          DepartmentId = departmentId,
          IsActive = true,
          IsPending = false,
          Role = request.Assignment == DepartmentUserAssignment.Director 
            ? (int)DepartmentUserRole.Manager
            : (int)request.Role,
          Assignment = (int)request.Assignment,
          CreatedBy = _httpContextAccessor.HttpContext.GetUserId()
        };
    }

    public DbDepartmentUser Map(ICreateDepartmentUserPublish request)
    {
      return request is null
        ? default
        : new DbDepartmentUser()
        {
          Id = Guid.NewGuid(),
          UserId = request.UserId,
          DepartmentId = request.DepartmentId,
          IsActive = request.IsActive,
          IsPending = true,
          Role = (int)DepartmentUserRole.Employee,
          Assignment = (int)DepartmentUserAssignment.Employee,
          CreatedBy = request.CreatedBy
        };
    }
  }
}
