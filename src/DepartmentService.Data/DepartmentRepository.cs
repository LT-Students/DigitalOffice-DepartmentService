﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Provider;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department.Filters;
using LT.DigitalOffice.Kernel.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
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

      if (filter.IncludeCategory)
      {
        dbDepartments = dbDepartments.Include(d => d.Category);
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

    public async Task<DbDepartment> GetAsync(GetDepartmentFilter filter, CancellationToken cancellationToken = default)
    {
      return filter is null
        ? null
        : await CreateGetPredicates(filter, _provider.Departments.AsQueryable())
          .FirstOrDefaultAsync(d => d.Id == filter.DepartmentId, cancellationToken);
    }

    public async Task<List<DbDepartment>> GetAsync(
      List<Guid> departmentsIds = null,
      List<Guid> usersIds = null)
    {
      IQueryable<DbDepartment> dbDepartments = _provider.Departments.AsQueryable();

      if (departmentsIds is not null && departmentsIds.Any())
      {
        dbDepartments = dbDepartments.Where(d => departmentsIds.Contains(d.Id));
      }

      if (usersIds is not null && usersIds.Any())
      {
        dbDepartments = dbDepartments.Where(d => d.Users.Any(du => du.IsActive && usersIds.Contains(du.UserId)));
      }

      dbDepartments = dbDepartments.Include(d => d.Users.Where(du => du.IsActive));

      return await dbDepartments.ToListAsync();
    }

    public async Task<(List<DbDepartment> dbDepartments, int totalCount)> FindAsync(FindDepartmentFilter filter, CancellationToken cancellationToken = default)
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
        dbDepartments = dbDepartments.Where(d =>
          d.Name.Contains(filter.NameIncludeSubstring, StringComparison.OrdinalIgnoreCase)
          || d.ShortName.Contains(filter.NameIncludeSubstring, StringComparison.OrdinalIgnoreCase));
      }

      return (
        await dbDepartments
          .Include(d => d.Category)
          .Include(d => d.Users.Where(u => u.IsActive))
          .Skip(filter.SkipCount)
          .Take(filter.TakeCount)
          .ToListAsync(cancellationToken),
        await dbDepartments.CountAsync(cancellationToken));
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

      request.ApplyTo(dbDepartment);
      dbDepartment.ModifiedBy = _httpContextAccessor.HttpContext.GetUserId();
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

    public async Task<List<Tuple<Guid, string, string, Guid?>>> GetDepartmentsTreeAsync(FindDepartmentFilter filter)
    {
      IQueryable<DbDepartment> departments = _provider.Departments.AsQueryable();

      if (filter.IsActive.HasValue)
      {
        departments = departments.Where(d => d.IsActive == filter.IsActive);
      }

      return await departments.Include(x => x.Category).Select(x => new Tuple<Guid, string, string, Guid?>(x.Id, x.Name, x.Category.Name, x.ParentId)).ToListAsync();
    }

    public async Task RemoveAsync(List<Guid> departmentsIds)
    {
      List<DbDepartment> dbDepartments = await _provider.Departments.Where(d => departmentsIds.Contains(d.Id)).ToListAsync();

      dbDepartments.ForEach(x => x.IsActive = false);

      await _provider.SaveAsync();
    }
  }
}
