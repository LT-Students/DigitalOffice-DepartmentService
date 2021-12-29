using System;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Provider;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Filters;
using LT.DigitalOffice.Kernel.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LT.DigitalOffice.DepartmentService.Data
{
  public class DepartmentProjectRepository : IDepartmentProjectRepository
  {
    private readonly IDataProvider _provider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DepartmentProjectRepository(
      IDataProvider provider,
      IHttpContextAccessor httpContextAccessor)
    {
      _provider = provider;
      _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Guid?> CreateAsync(DbDepartmentProject dbDepartmentProject)
    {
      if (dbDepartmentProject == null)
      {
        return null;
      }

      _provider.DepartmentsProjects.Add(dbDepartmentProject);
      await _provider.SaveAsync();

      return dbDepartmentProject.Id;
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

    public async Task RemoveAsync(Guid projectId)
    {
      DbDepartmentProject dbDepartmentProject = await _provider.DepartmentsProjects
        .FirstOrDefaultAsync(dp => dp.ProjectId == projectId && dp.IsActive);

      if (dbDepartmentProject != null)
      {
        dbDepartmentProject.IsActive = false;
        dbDepartmentProject.ModifiedAtUtc = DateTime.UtcNow;
        dbDepartmentProject.ModifiedBy = _httpContextAccessor.HttpContext.GetUserId();
        await _provider.SaveAsync();
      }
    }
  }
}
