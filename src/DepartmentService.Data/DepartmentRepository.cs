using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Provider;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Filters;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Models.Broker.Requests.Department;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.EntityFrameworkCore;

namespace LT.DigitalOffice.DepartmentService.Data
{
  public class DepartmentRepository : IDepartmentRepository
  {
    private readonly IDataProvider _provider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DepartmentRepository(
      IDataProvider provider,
      IHttpContextAccessor httpContextAccessor)
    {
      _provider = provider;
      _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Guid?> CreateAsync(DbDepartment dbDepartment)
    {
      if (dbDepartment == null)
      {
        return null;
      }

      _provider.Departments.Add(dbDepartment);
      await _provider.SaveAsync();

      return dbDepartment.Id;
    }

    public async Task<DbDepartment> GetAsync(GetDepartmentFilter filter)
    {
      IQueryable<DbDepartment> dbDepartments = _provider.Departments.AsQueryable();

      dbDepartments = dbDepartments.Where(d => d.Id == filter.DepartmentId);

      if (filter.IncludeUsers)
      {
        dbDepartments = dbDepartments.Include(d => d.Users.Where(u => u.IsActive));
      }

      if (filter.IncludeProjects)
      {
        dbDepartments = dbDepartments.Include(d => d.Projects.Where(p => p.IsActive));
      }

      if (filter.IncludeNews)
      {
        dbDepartments = dbDepartments.Include(d => d.News.Where(n => n.IsActive));
      }

      return await dbDepartments.FirstOrDefaultAsync();
    }

    public async Task<(List<DbDepartment> dbDepartments, int totalCount)> FindAsync(FindDepartmentFilter filter)
    {
      IQueryable<DbDepartment> dbDepartments = _provider.Departments.AsQueryable();

      if (!filter.IncludeDeactivated)
      {
        dbDepartments = dbDepartments.Where(d => d.IsActive);
      }

      return (
        await dbDepartments
          .Include(d => d.Users.Where(u => u.IsActive))
          .Skip(filter.SkipCount)
          .Take(filter.TakeCount)
          .ToListAsync(),
        await dbDepartments.CountAsync());
    }
    public async Task<List<DbDepartment>> GetDepartmenDataAsync(List<Guid> departmentIds)
    {
      return await _provider.Departments.
        Where(d => departmentIds.Contains(d.Id)).Include(d => d.Users.Where(u => u.IsActive))
        .ToListAsync();
    }

    public async Task<List<DbDepartment>> GetAsync(List<Guid> departmentsIds, bool includeUsers = false)
    {
      IQueryable<DbDepartment> departments = _provider.Departments.Where(d => departmentsIds.Contains(d.Id));

      if (includeUsers)
      {
        departments = departments.Include(d => d.Users.Where(u => u.IsActive));
      }

      return await departments.ToListAsync();
    }

    public async Task<List<DbDepartment>> GetAsync(IGetDepartmentsRequest request)
    {
      IQueryable<DbDepartment> dbDepartments = _provider.Departments.AsQueryable();

      if (request.NewsIds is not null && request.NewsIds.Any())
      {
        dbDepartments = dbDepartments.Include(d => d.News.Where(dn => dn.IsActive));
        dbDepartments = dbDepartments.Where(d => d.News.Any(dn => request.NewsIds.Contains(dn.NewsId)));
      }

      if (request.ProjectsIds is not null && request.ProjectsIds.Any())
      {
        dbDepartments = dbDepartments.Include(d => d.Projects.Where(dp => dp.IsActive));
        dbDepartments = dbDepartments.Where(d => d.Projects.Any(dp => request.ProjectsIds.Contains(dp.ProjectId)));
      }

      if (request.UsersIds is not null && request.UsersIds.Any())
      {
        dbDepartments = dbDepartments.Where(d => d.Users.Any(du => du.IsActive && request.UsersIds.Contains(du.UserId)));
      }

      dbDepartments = dbDepartments.Include(d => d.Users.Where(du => du.IsActive));

      return await dbDepartments.ToListAsync();
    }

    public async Task<List<DbDepartment>> SearchAsync(string text)
    {
      return await _provider.Departments
        .Where(d => d.Name.Contains(text, StringComparison.OrdinalIgnoreCase))
        .ToListAsync();
    }

    public async Task<bool> EditAsync(Guid departmentId, JsonPatchDocument<DbDepartment> request)
    {
      DbDepartment dbDepartment = _provider.Departments.FirstOrDefault(x => x.Id == departmentId);

      if (dbDepartment == null || request == null)
      {
        return false;
      }

      Guid editorId = _httpContextAccessor.HttpContext.GetUserId();

      Operation<DbDepartment> deactivatedOperation = request.Operations
        .FirstOrDefault(o => o.path.EndsWith(nameof(DbDepartment.IsActive), StringComparison.OrdinalIgnoreCase));

      if (deactivatedOperation != null && !bool.Parse(deactivatedOperation.value.ToString()) && dbDepartment.IsActive)
      {
        List<DbDepartmentUser> users = await _provider.DepartmentsUsers
          .Where(u => u.IsActive && u.DepartmentId == dbDepartment.Id)
          .ToListAsync();

        foreach (DbDepartmentUser user in users)
        {
          user.IsActive = false;
          user.ModifiedAtUtc = DateTime.UtcNow;
          user.LeftAtUtc = DateTime.UtcNow;
          user.ModifiedBy = editorId;
        }
      }

      request.ApplyTo(dbDepartment);
      dbDepartment.ModifiedBy = editorId;
      dbDepartment.ModifiedAtUtc = DateTime.UtcNow;
      await _provider.SaveAsync();

      return true;
    }

    public async Task<bool> NameExistAsync(string name)
    {
      return await _provider.Departments.AnyAsync(d => d.Name == name);
    }

    public async Task<bool> ExistAsync(Guid departmentId)
    {
      return await _provider.Departments.AnyAsync(x => x.Id == departmentId);
    }
  }
}
