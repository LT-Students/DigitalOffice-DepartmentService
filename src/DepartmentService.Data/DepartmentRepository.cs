using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Provider;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Filters;
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

    public DepartmentRepository(
      IDataProvider provider,
      IHttpContextAccessor httpContextAccessor)
    {
      _provider = provider;
      _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Guid> CreateAsync(DbDepartment department)
    {
      _provider.Departments.Add(department);
      await _provider.SaveAsync();

      return department.Id;
    }

    public async Task<DbDepartment> GetAsync(GetDepartmentFilter filter)
    {
      IQueryable<DbDepartment> dbDepartments = _provider.Departments.AsQueryable();

      dbDepartments = dbDepartments.Where(d => d.Id == filter.DepartmentId);

      if (filter.IsIncludeUsers)
      {
        dbDepartments = dbDepartments.Include(d => d.Users.Where(u => u.IsActive));
      }

      return await dbDepartments.FirstOrDefaultAsync();
    }

    public async Task<(List<DbDepartment>, int totalCount)> FindAsync(FindDepartmentFilter filter)
    {
      return (
        await _provider.Departments
          .Skip(filter.SkipCount)
          .Take(filter.TakeCount)
          .Include(d => d.Users.Where(u => u.IsActive))
          .ToListAsync(),
        await _provider.Departments.CountAsync());
    }

    public async Task<List<DbDepartment>> GetAsync(List<Guid> departmentIds, bool includeUsers = false)
    {
      IQueryable<DbDepartment> departments = _provider.Departments.Where(d => departmentIds.Contains(d.Id));

      if (includeUsers)
      {
        departments = departments.Include(d => d.Users);
      }

      return await departments.ToListAsync();
    }

    public List<DbDepartment> Search(string text)
    {
      return _provider.Departments.ToList().Where(d => d.Name.Contains(text, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<bool> EditAsync(DbDepartment department, JsonPatchDocument<DbDepartment> request)
    {
      if (department == null)
      {
        return false;
      }

      Guid editorId = _httpContextAccessor.HttpContext.GetUserId();

      Operation<DbDepartment> deactivatedOperation = request.Operations
        .FirstOrDefault(o => o.path.EndsWith(nameof(DbDepartment.IsActive), StringComparison.OrdinalIgnoreCase));
      if (deactivatedOperation != null && !bool.Parse(deactivatedOperation.value.ToString()))
      {
        List<DbDepartmentUser> users = await _provider.DepartmentUsers
          .Where(u => u.IsActive && u.DepartmentId == department.Id)
          .ToListAsync();

        foreach (DbDepartmentUser user in users)
        {
          user.IsActive = false;
          user.ModifiedAtUtc = DateTime.UtcNow;
          user.ModifiedBy = editorId;
        }
      }

      request.ApplyTo(department);
      department.ModifiedBy = editorId;
      department.ModifiedAtUtc = DateTime.UtcNow;
      await _provider.SaveAsync();

      return true;
    }

    public async Task<bool> DoesNameExistAsync(string name)
    {
      return await _provider.Departments.AnyAsync(d => d.Name == name);
    }
  }
}
