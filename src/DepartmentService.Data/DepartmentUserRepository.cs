using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DigitalOffice.Models.Broker.Publishing;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Provider;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.DepartmentUser;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Models.Broker.Enums;
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

    public async Task<List<Guid>> EditAsync(List<DbDepartmentUser> departmentUsers)
    {
      if (departmentUsers is null || !departmentUsers.Any())
      {
        return null;
      }

      List<DbDepartmentUser> dbDepartmentsUsers = await _provider.DepartmentsUsers
        .Where(du => departmentUsers.Select(r => r.UserId).Contains(du.UserId)).ToListAsync();

      if (dbDepartmentsUsers.Any())
      {
        DbDepartmentUser requestData = null;

        foreach (DbDepartmentUser du in dbDepartmentsUsers)
        {
          requestData = departmentUsers.FirstOrDefault(u => u.UserId == du.UserId);

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

    public async Task<Guid?> ActivateAsync(IActivateUserPublish request)
    {
      DbDepartmentUser user = await _provider.DepartmentsUsers.FirstOrDefaultAsync(u => u.UserId == request.UserId && !u.IsActive);

      if (user is null)
      {
        return null;
      }

      user.IsActive = true;
      user.IsPending = false;

      await _provider.SaveAsync();

      return user.DepartmentId;
    }

    public async Task<Guid?> MakeUserPendingAsync(Guid userId, Guid createdBy)
    {
      DbDepartmentUser dbDepartmentUser = await _provider.DepartmentsUsers.FirstOrDefaultAsync(du => du.UserId == userId);

      if (dbDepartmentUser is null)
      {
        return null;
      }

      dbDepartmentUser.IsActive = false;
      dbDepartmentUser.IsPending = true;
      dbDepartmentUser.CreatedBy = createdBy;

      await _provider.SaveAsync();

      return dbDepartmentUser.DepartmentId;
    }

    public async Task<bool> EditRoleAsync(List<Guid> usersIds, DepartmentUserRole role)
    {
      if (usersIds is null || !usersIds.Any())
      {
        return false;
      }

      List<DbDepartmentUser> dbDepartmentsUsers = await _provider.DepartmentsUsers
        .Where(du => usersIds.Contains(du.UserId)).ToListAsync();

      if (dbDepartmentsUsers.Any())
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

      List<DbDepartmentUser> dbDepartmentUsers =
        await _provider.DepartmentsUsers.Where(du => du.DepartmentId == departmentId
          && usersIds.Contains(du.UserId)
          && du.IsActive)
          .ToListAsync();

      if (!dbDepartmentUsers.Any())
      {
        return false;
      }

      foreach (DbDepartmentUser du in dbDepartmentUsers)
      {
        du.Assignment = (int)assignment;
        du.Role = assignment == DepartmentUserAssignment.Director ? (int)DepartmentUserRole.Manager : (int)DepartmentUserRole.Employee;
        du.CreatedBy = _httpContextAccessor.HttpContext.GetUserId();
      }

      await _provider.SaveAsync();
      return true;
    }

    public Task<List<DbDepartmentUser>> GetAsync(Guid departmentId, FindDepartmentUsersFilter filter, CancellationToken cancellationToken = default)
    {
      IQueryable<DbDepartmentUser> departmentUsersQuery = _provider.DepartmentsUsers.Where(du => du.DepartmentId == departmentId);

      if (filter.IsActive.HasValue)
      {
        departmentUsersQuery = departmentUsersQuery.Where(pu => pu.IsActive == filter.IsActive.Value);
      }

      if (filter.DepartmentUserRoleAscendingSort.HasValue)
      {
        departmentUsersQuery = filter.DepartmentUserRoleAscendingSort.Value
          ? departmentUsersQuery.OrderBy(d => d.Assignment).ThenBy(d => d.Role)
          : departmentUsersQuery.OrderByDescending(d => d.Assignment).ThenByDescending(d => d.Role);
      }

      return departmentUsersQuery.ToListAsync(cancellationToken);
    }

    public Task<DbDepartmentUser> GetAsync(Guid userId, bool includeDepartment = false)
    {
      return CreateGetPredicates(includeDepartment, _provider.DepartmentsUsers.AsQueryable())
        .FirstOrDefaultAsync(u => u.IsActive && u.UserId == userId);
    }

    public Task<List<DbDepartmentUser>> GetAsync(List<Guid> usersIds, bool includeDepartments = false)
    {
      return usersIds is null
        ? Task.FromResult(default(List<DbDepartmentUser>))
        : CreateGetPredicates(includeDepartments, _provider.DepartmentsUsers.AsQueryable())
          .Where(u => u.IsActive && usersIds.Contains(u.UserId))
          .ToListAsync();
    }

    public Task<List<DbDepartmentUser>> GetAsync(IGetDepartmentsUsersRequest request)
    {
      IQueryable<DbDepartmentUser> departmentUsersQuery = request.ByEntryDate.HasValue 
        ? _provider.DepartmentsUsers
            .TemporalBetween(
              request.ByEntryDate.Value,
              request.ByEntryDate.Value.AddMonths(1))
        : _provider.DepartmentsUsers.AsQueryable();

      departmentUsersQuery = departmentUsersQuery.Where(du => request.DepartmentsIds.Contains(du.DepartmentId));

      if (request.IncludePendingUsers)
      {
        departmentUsersQuery = from users in departmentUsersQuery.Where(du => du.IsActive || du.IsPending)
                               group users by users.UserId into userGroup
                               select userGroup.AsQueryable().OrderByDescending(u => u.IsActive).First();
      }
      else
      {
        departmentUsersQuery = departmentUsersQuery.Where(du => du.IsActive).Distinct();
      }

      return departmentUsersQuery.ToListAsync();
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

      if (targetUsers.Any())
      {
        foreach (DbDepartmentUser du in targetUsers)
        {
          du.IsActive = false;
          du.CreatedBy = _httpContextAccessor.HttpContext.GetUserId(); 
        }

        await _provider.SaveAsync();
      }
    }

    public async Task RemoveAsync(List<Guid> departmentIds)
    {
      List<DbDepartmentUser> dbDepartmentUsers = await _provider.DepartmentsUsers.Where(du => departmentIds.Contains(du.DepartmentId)).ToListAsync();

      foreach (DbDepartmentUser dbDepartmentUser in dbDepartmentUsers)
      {
        dbDepartmentUser.IsActive = false;
        dbDepartmentUser.CreatedBy = _httpContextAccessor.HttpContext.GetUserId();
      }

      await _provider.SaveAsync();
    }

    public async Task RemoveDirectorAsync(Guid departmentId)
    {
      DbDepartmentUser director = await
        _provider.DepartmentsUsers.FirstOrDefaultAsync(du => du.DepartmentId == departmentId
          && du.Assignment == (int)DepartmentUserAssignment.Director
          && du.IsActive);

      if (director is not null)
      {
        director.Assignment = (int)DepartmentUserAssignment.Employee;
        director.Role = (int)DepartmentUserRole.Employee;
        director.CreatedBy = _httpContextAccessor.HttpContext.GetUserId();

        await _provider.SaveAsync();
      }
    }
  }
}
