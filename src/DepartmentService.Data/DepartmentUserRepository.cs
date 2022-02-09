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

    public DepartmentUserRepository(
      IHttpContextAccessor httpContextAccessor,
      IDataProvider provider)
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

    public async Task<Guid?> CreateAsync(DbDepartmentUser dbDepartmentUser)
    {
      if (dbDepartmentUser is null)
      {
        return null;
      }

      _provider.DepartmentsUsers.Add(dbDepartmentUser);
      await _provider.SaveAsync();

      return dbDepartmentUser.Id;
    }

    public async Task<DbDepartmentUser> GetAsync(Guid userId, bool includeDepartment = false)
    {
      IQueryable<DbDepartmentUser> dbDepartmentUser = _provider.DepartmentsUsers.AsQueryable();

      if (includeDepartment)
      {
        dbDepartmentUser = dbDepartmentUser.Include(du => du.Department);
      }

      return await dbDepartmentUser
        .FirstOrDefaultAsync(u => u.IsActive && u.UserId == userId);
    }

    public async Task<bool> ChangeDirectorAsync(Guid departmentId, Guid newDirectorId)
    {
      List<DbDepartmentUser> directors =
        _provider.DepartmentsUsers.Where(du => du.DepartmentId == departmentId
          && (du.Role == (int)DepartmentUserRole.Director || du.UserId == newDirectorId)
          && du.IsActive).ToList();

      if (!directors.Any())
      {
        return false;
      }

      DbDepartmentUser prevDirector = directors
        .FirstOrDefault(d => d.Role == (int)DepartmentUserRole.Director);
      DbDepartmentUser newDirector = directors
        .FirstOrDefault(d => d.Role == (int)DepartmentUserRole.Employee);

      if (newDirector == null)
      {
        return false;
      }

      if (prevDirector != null)
      {
        prevDirector.Role = (int)DepartmentUserRole.Employee;
      }

      newDirector.Role = (int)DepartmentUserRole.Director;

      await _provider.SaveAsync();

      return true;
    }

    public async Task<(List<Guid> usersIds, int totalCount)> GetAsync(IGetDepartmentUsersRequest request)
    {
      IQueryable<DbDepartmentUser> dbDepartmentUser = _provider.DepartmentsUsers.AsQueryable();

      dbDepartmentUser = dbDepartmentUser.Where(x => x.DepartmentId == request.DepartmentId);

      if (request.ByEntryDate.HasValue)
      {
        dbDepartmentUser = dbDepartmentUser.Where(x =>
          ((x.CreatedAtUtc.Year * 12 + x.CreatedAtUtc.Month) <=
            (request.ByEntryDate.Value.Year * 12 + request.ByEntryDate.Value.Month)) &&
          (x.IsActive ||
            ((x.LeftAtUtc.Value.Year * 12 + x.LeftAtUtc.Value.Month) >=
            (request.ByEntryDate.Value.Year * 12 + request.ByEntryDate.Value.Month))));
      }
      else
      {
        dbDepartmentUser = dbDepartmentUser.Where(x => x.IsActive);
      }

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

    public async Task<List<DbDepartmentUser>> GetAsync(List<Guid> usersIds, bool includeDepartments = false)
    {
      if (usersIds is null)
      {
        return null;
      }

      return await _provider.DepartmentsUsers
        .Include(du => du.Department)
        .Where(u => u.IsActive && usersIds.Contains(u.UserId))
        .ToListAsync();
    }

    public async Task<List<Guid>> RemoveAsync(List<Guid> usersIds)
    {
      List<DbDepartmentUser> dbDepartmentsUsers = await _provider.DepartmentsUsers
        .Where(du => du.IsActive && usersIds.Contains(du.UserId)).ToListAsync();

      if (dbDepartmentsUsers != null && dbDepartmentsUsers.Any())
      {
        foreach (DbDepartmentUser du in dbDepartmentsUsers)
        {
          du.IsActive = false;
          du.ModifiedAtUtc = DateTime.UtcNow;
          du.ModifiedBy = _httpContextAccessor.HttpContext.GetUserId();
          du.LeftAtUtc = DateTime.UtcNow;
        };

        await _provider.SaveAsync();
      }

      return dbDepartmentsUsers.Select(du => du.DepartmentId).ToList();
    }

    public async Task<Guid?> RemoveAsync(Guid userId, Guid removedBy)
    {
      DbDepartmentUser dbDepartmentUser = await _provider.DepartmentsUsers
        .FirstOrDefaultAsync(du => du.UserId == userId && du.IsActive);

      if (dbDepartmentUser is null)
      {
        return null;
      }

      dbDepartmentUser.IsActive = false;
      dbDepartmentUser.ModifiedAtUtc = DateTime.UtcNow;
      dbDepartmentUser.ModifiedBy = removedBy;
      dbDepartmentUser.LeftAtUtc = DateTime.UtcNow;

      await _provider.SaveAsync();

      return dbDepartmentUser.DepartmentId;
    }

    public async Task<bool> RemoveAsync(Guid departmentId, IEnumerable<Guid> usersIds)
    {
      List<DbDepartmentUser> dbDepartmentUsers = await _provider.DepartmentsUsers
        .Where(du => du.IsActive && du.DepartmentId == departmentId && usersIds.Contains(du.UserId))
        .ToListAsync();

      if (!dbDepartmentUsers.Any())
      {
        return false;
      }

      foreach (DbDepartmentUser dbDepartmentUser in dbDepartmentUsers)
      {
        dbDepartmentUser.IsActive = false;
        dbDepartmentUser.ModifiedAtUtc = DateTime.UtcNow;
        dbDepartmentUser.ModifiedBy = _httpContextAccessor.HttpContext.GetUserId();
        dbDepartmentUser.LeftAtUtc = DateTime.UtcNow;
      }

      _provider.DepartmentsUsers.UpdateRange(dbDepartmentUsers);
      await _provider.SaveAsync();

      return true;
    }
  }
}
