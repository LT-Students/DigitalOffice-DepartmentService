using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Provider;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department.Filters;
using LT.DigitalOffice.Kernel.Extensions;
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

    private IQueryable<DbDepartment> CreateGetPredicates(
      GetDepartmentFilter filter,
      IQueryable<DbDepartment> dbDepartments)
    {
      if (filter.IncludeUsers)
      {
        dbDepartments = dbDepartments.Include(d => d.Users.Where(u => u.IsActive));
      }

      if (filter.IncludeProjects)
      {
        dbDepartments = dbDepartments.Include(d => d.Projects.Where(p => p.IsActive));
      }

      return dbDepartments;
    }

    public DepartmentRepository(
      IDataProvider provider,
      IHttpContextAccessor httpContextAccessor)
    {
      _provider = provider;
      _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Guid?> CreateAsync(DbDepartment dbDepartment)
    {
      if (dbDepartment is null)
      {
        return null;
      }

      _provider.Departments.Add(dbDepartment);
      await _provider.SaveAsync();

      return dbDepartment.Id;
    }

    public async Task<DbDepartment> GetAsync(GetDepartmentFilter filter)
    {
      return filter is null
        ? null
        : await CreateGetPredicates(filter, _provider.Departments.AsQueryable())
          .FirstOrDefaultAsync(d => d.Id == filter.DepartmentId);
    }

    public async Task<List<DbDepartment>> GetAsync(
      List<Guid> departmentsIds = null,
      List<Guid> usersIds = null,
      List<Guid> projectsIds = null)
    {
      IQueryable<DbDepartment> dbDepartments = _provider.Departments.AsQueryable();

      if (departmentsIds is not null && departmentsIds.Any())
      {
        dbDepartments = dbDepartments.Where(d => departmentsIds.Contains(d.Id));
      }

      if (projectsIds is not null && projectsIds.Any())
      {
        dbDepartments = dbDepartments.Include(d => d.Projects.Where(dp => dp.IsActive));
        dbDepartments = dbDepartments.Where(d => d.Projects.Any(dp => projectsIds.Contains(dp.ProjectId)));
      }

      if (usersIds is not null && usersIds.Any())
      {
        dbDepartments = dbDepartments.Where(d => d.Users.Any(du => du.IsActive && usersIds.Contains(du.UserId)));
      }

      dbDepartments = dbDepartments.Include(d => d.Users.Where(du => du.IsActive));

      return await dbDepartments.ToListAsync();
    }


    public async Task<(List<DbDepartment> dbDepartments, int totalCount)> FindAsync(FindDepartmentFilter filter)
    {
      IQueryable<DbDepartment> dbDepartments = _provider.Departments.AsQueryable();

      if (filter.IsActive.HasValue)
      {
        dbDepartments = dbDepartments.Where(d => d.IsActive == filter.IsActive);
      }

      if (filter.IsAscendingSort.HasValue)
      {
        dbDepartments = filter.IsAscendingSort.Value
          ? dbDepartments.OrderBy(d => d.Name)
          : dbDepartments.OrderByDescending(d => d.Name);
      }

      if (!string.IsNullOrWhiteSpace(filter.NameIncludeSubstring))
      {
        dbDepartments = dbDepartments.Where(d => d.Name.ToLower().Contains(filter.NameIncludeSubstring.ToLower()));
      }

      return (
        await dbDepartments
          .Include(d => d.Users.Where(u => u.IsActive))
          .Skip(filter.SkipCount)
          .Take(filter.TakeCount)
          .ToListAsync(),
        await dbDepartments.CountAsync());
    }

    public async Task<List<DbDepartment>> SearchAsync(string text)
    {
      return await _provider.Departments
        .Where(d => d.Name.Contains(text, StringComparison.OrdinalIgnoreCase))
        .ToListAsync();
    }

    public async Task<bool> EditAsync(Guid departmentId, JsonPatchDocument<DbDepartment> request)
    {
      DbDepartment dbDepartment = await _provider.Departments.FirstOrDefaultAsync(x => x.Id == departmentId);

      if (dbDepartment is null || request is null)
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

    public async Task<bool> ShortNameExistAsync(string shortName)
    {
      return await _provider.Departments.AnyAsync(d => d.ShortName == shortName);
    }

    public async Task<bool> ExistAsync(Guid departmentId)
    {
      return await _provider.Departments.AnyAsync(x => x.Id == departmentId && x.IsActive);
    }
  }
}
