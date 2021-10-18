using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Provider;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.Models.Broker.Requests.Company;
using Microsoft.EntityFrameworkCore;

namespace LT.DigitalOffice.DepartmentService.Data
{
  public class DepartmentUserRepository : IDepartmentUserRepository
  {
    private readonly IDataProvider _provider;

    public DepartmentUserRepository(
      IDataProvider provider)
    {
      _provider = provider;
    }

    public async Task<bool> AddAsync(DbDepartmentUser departmentUser)
    {
      if (departmentUser == null)
      {
        return false;
      }

      _provider.DepartmentUsers.Add(departmentUser);
      await _provider.SaveAsync();

      return true;
    }

    public async Task<bool> AddAsync(List<DbDepartmentUser> departmentUsers)
    {
      if (departmentUsers == null || !departmentUsers.Any())
      {
        return false;
      }

      _provider.DepartmentUsers.AddRange(departmentUsers);
      await _provider.SaveAsync();

      return true;
    }

    public async Task<DbDepartmentUser> GetAsync(Guid userId, bool includeDepartment)
    {
      DbDepartmentUser user = null;

      if (includeDepartment)
      {
        user = await _provider.DepartmentUsers.Include(u => u.Department).FirstOrDefaultAsync(u => u.IsActive && u.UserId == userId);
      }
      else
      {
        user = await _provider.DepartmentUsers.FirstOrDefaultAsync(u => u.IsActive && u.UserId == userId);
      }

      if (user == null)
      {
        return null;
      }

      return user;
    }

    public async Task<bool> ChangeDirectorAsync(Guid departmentId, Guid newDirectorId)
    {
      List<DbDepartmentUser> directors =
        _provider.DepartmentUsers.Where(du => du.DepartmentId == departmentId
          && (du.Role == (int)DepartmentUserRole.Director || du.UserId == newDirectorId)
          && du.IsActive).ToList();

      if (!directors.Any())
      {
        return false;
      }

      DbDepartmentUser prevDirector = directors.FirstOrDefault(d => d.Role == (int)DepartmentUserRole.Director);
      DbDepartmentUser newDirector = directors.FirstOrDefault(d => d.Role == (int)DepartmentUserRole.Employee);

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
      IQueryable<DbDepartmentUser> dbDepartmentUser = _provider.DepartmentUsers.AsQueryable();

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

    public async Task<List<DbDepartmentUser>> GetAsync(List<Guid> userIds)
    {
      return await _provider.DepartmentUsers
        .Include(du => du.Department)
        .Where(u => u.IsActive && userIds.Contains(u.UserId))
        .ToListAsync();
    }

    public async Task RemoveAsync(Guid userId, Guid removedBy)
    {
      DbDepartmentUser dbDepartmentUser = await _provider.DepartmentUsers
        .FirstOrDefaultAsync(du => du.UserId == userId && du.IsActive);

      if (dbDepartmentUser != null)
      {
        dbDepartmentUser.IsActive = false;
        dbDepartmentUser.ModifiedAtUtc = DateTime.UtcNow;
        dbDepartmentUser.ModifiedBy = removedBy;
        dbDepartmentUser.LeftAtUtc = DateTime.UtcNow;

        await _provider.SaveAsync();
      }
    }

    public async Task RemoveAsync(List<Guid> usersIds, Guid removedBy)
    {
      List<DbDepartmentUser> dbDepartmentsUsers = await _provider.DepartmentUsers
        .Where(du => du.IsActive && usersIds.Contains(du.UserId)).ToListAsync();

      if (usersIds != null && usersIds.Any())
      {
        foreach (DbDepartmentUser du in dbDepartmentsUsers)
        {
          du.IsActive = false;
          du.ModifiedAtUtc = DateTime.UtcNow;
          du.ModifiedBy = removedBy;
          du.LeftAtUtc = DateTime.UtcNow;
        };

        await _provider.SaveAsync();
      }
    }
  }
}
