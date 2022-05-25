using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Provider;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Models.Broker.Requests.Department;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LT.DigitalOffice.DepartmentService.Data
{
  public class DepartmentUserRepository : IDepartmentUserRepository
  {
    private readonly IDataProvider _provider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private IQueryable<DbDepartmentUser> CreateGetPredicates(
      bool includeDepartments,
      IQueryable<DbDepartmentUser> dbDepartmentUsers)
    {
      if (includeDepartments)
      {
        dbDepartmentUsers = dbDepartmentUsers.Include(du => du.Department);
      }

      return dbDepartmentUsers;
    }

    public DepartmentUserRepository(
      IDataProvider provider,
      IHttpContextAccessor httpContextAccessor)
    {
      _provider = provider;
      _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> CreateAsync(List<DbDepartmentUser> departmentsUsers)
    {
      if (departmentsUsers is null || !departmentsUsers.Any())
      {
        return false;
      }

      _provider.DepartmentsUsers.AddRange(departmentsUsers);
      await _provider.SaveAsync();

      return true;
    }

    public async Task<List<Guid>> EditAsync(List<DbDepartmentUser> request)
    {
      if (request is null || !request.Any())
      {
        return null;
      }

      IQueryable<DbDepartmentUser> dbDepartmentsUsers = _provider.DepartmentsUsers
        .Where(du => request.Select(r => r.UserId).Contains(du.UserId));

      if (dbDepartmentsUsers is not null && dbDepartmentsUsers.Any())
      {
        DbDepartmentUser requestData = null;

        foreach (DbDepartmentUser du in dbDepartmentsUsers)
        {
          requestData = request.FirstOrDefault(u => u.UserId == du.UserId);

          du.DepartmentId = requestData.DepartmentId;
          du.CreatedBy = requestData.CreatedBy;
          du.Assignment = requestData.Assignment;
          du.Role = requestData.Role;
          du.IsActive = requestData.IsActive;
        }

        await _provider.SaveAsync();
      }

      return dbDepartmentsUsers?.Select(u => u.UserId).ToList();
    }

    public async Task<bool> EditRoleAsync(List<Guid> usersIds, DepartmentUserRole role)
    {
      if (usersIds is null || !usersIds.Any())
      {
        return false;
      }

      IQueryable<DbDepartmentUser> dbDepartmentsUsers = _provider.DepartmentsUsers
        .Where(du => usersIds.Contains(du.UserId));

      if (dbDepartmentsUsers is null && dbDepartmentsUsers.Any())
      {
        return false;
      }

      foreach (DbDepartmentUser du in dbDepartmentsUsers)
      {
        du.Role = (int)role;
        du.CreatedBy = _httpContextAccessor.HttpContext.GetUserId();
      }

      await _provider.SaveAsync();

      return true;
    }

    public async Task<bool> EditAssignmentAsync(Guid departmentId, List<Guid> usersIds, DepartmentUserAssignment assignment)
    {
      if (usersIds is null || !usersIds.Any())
      {
        return false;
      }

      IQueryable<DbDepartmentUser> dbDepartmentUsers =
        _provider.DepartmentsUsers.Where(du => du.DepartmentId == departmentId
          && usersIds.Contains(du.UserId)
          && du.IsActive);

      if (dbDepartmentUsers is null || !dbDepartmentUsers.Any())
      {
        return false;
      }

      foreach (DbDepartmentUser du in dbDepartmentUsers)
      {
        du.Assignment = (int)assignment;
        du.CreatedBy = _httpContextAccessor.HttpContext.GetUserId();
      }

      await _provider.SaveAsync();

      return true;
    }

    public async Task<DbDepartmentUser> GetAsync(Guid userId, bool includeDepartment = false)
    {
      return await
        CreateGetPredicates(includeDepartment, _provider.DepartmentsUsers.AsQueryable())
        .FirstOrDefaultAsync(u => u.IsActive && u.UserId == userId);
    }

    public async Task<List<DbDepartmentUser>> GetAsync(List<Guid> usersIds, bool includeDepartments = false)
    {
      return usersIds is null
        ? new()
        : await
          CreateGetPredicates(includeDepartments, _provider.DepartmentsUsers.AsQueryable())
          .Where(u => u.IsActive && usersIds.Contains(u.UserId))
          .ToListAsync();
    }

    public async Task<(List<Guid> usersIds, int totalCount)> GetAsync(IGetDepartmentUsersRequest request)
    {
      IQueryable<DbDepartmentUser> dbDepartmentUser = request.ByEntryDate.HasValue 
        ? _provider.DepartmentsUsers
            .TemporalBetween(
              request.ByEntryDate.Value,
              new DateTime(request.ByEntryDate.Value.Year, request.ByEntryDate.Value.Month + 1, 1))
            .AsQueryable()
        : _provider.DepartmentsUsers.AsQueryable();

      dbDepartmentUser = dbDepartmentUser.Where(du => du.DepartmentId == request.DepartmentId && du.IsActive);

      int totalCount = await dbDepartmentUser.CountAsync();

      if (request.SkipCount.HasValue)
      {
        dbDepartmentUser = dbDepartmentUser.Skip(request.SkipCount.Value);
      }

      if (request.TakeCount.HasValue)
      {
        dbDepartmentUser = dbDepartmentUser.Take(request.TakeCount.Value);
      }

      return (await dbDepartmentUser.Select(x => x.UserId).ToListAsync(), totalCount);
    }

    public async Task<Guid?> RemoveAsync(Guid userId, Guid removedBy)
    {
      DbDepartmentUser dbDepartmentUser = await _provider.DepartmentsUsers
        .FirstOrDefaultAsync(du => du.UserId == userId);

      if (dbDepartmentUser is null)
      {
        return null;
      }

      dbDepartmentUser.IsActive = false;
      dbDepartmentUser.CreatedBy = removedBy;

      await _provider.SaveAsync();

      return dbDepartmentUser.DepartmentId;
    }

    public async Task RemoveAsync(Guid departmentId, List<Guid> usersIds = null)
    {
      IQueryable<DbDepartmentUser> dbDepartmentUsers = _provider.DepartmentsUsers.AsQueryable();

      dbDepartmentUsers = dbDepartmentUsers.Where(du => du.DepartmentId == departmentId && du.IsActive);

      if (usersIds is not null && usersIds.Any())
      {
        dbDepartmentUsers = dbDepartmentUsers.Where(du => usersIds.Contains(du.UserId));
      }

      List<DbDepartmentUser> targetUsers = await dbDepartmentUsers.ToListAsync();

      if (targetUsers is not null || targetUsers.Any())
      {
        foreach (DbDepartmentUser du in targetUsers)
        {
          du.IsActive = false;
          du.CreatedBy = _httpContextAccessor.HttpContext.GetUserId(); 
        }

        await _provider.SaveAsync();
      }
    }

    public async Task RemoveDirectorAsync(Guid departmentId)
    {
      DbDepartmentUser director = await
        _provider.DepartmentsUsers.FirstOrDefaultAsync(du => du.DepartmentId == departmentId
          && du.Assignment == (int)DepartmentUserAssignment.Director
          && du.IsActive);

      if (director is not null)
      {
        director.Assignment = (int)DepartmentUserAssignment.User;
        director.CreatedBy = _httpContextAccessor.HttpContext.GetUserId();

        await _provider.SaveAsync();
      }
    }

    public async Task<bool> IsManagerAsync(Guid userId)
    {
      DbDepartmentUser dbDepartmentUser = await _provider.DepartmentsUsers
        .FirstOrDefaultAsync(du => du.UserId == userId);

      return dbDepartmentUser is not null
        && dbDepartmentUser.IsActive
        && dbDepartmentUser.Assignment == (int)DepartmentUserRole.Manager;
    }
  }
}
