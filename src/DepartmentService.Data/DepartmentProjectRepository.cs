using System;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Provider;
using LT.DigitalOffice.DepartmentService.Models.Db;
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
      if (dbDepartmentProject is null)
      {
        return null;
      }

      _provider.DepartmentsProjects.Add(dbDepartmentProject);
      await _provider.SaveAsync();

      return dbDepartmentProject.Id;
    }

    public async Task<bool> EditAsync(Guid projectId, Guid? departmentId)
    {
      DbDepartmentProject dbDepartmentProject = await _provider.DepartmentsProjects
        .FirstOrDefaultAsync(dp => dp.ProjectId == projectId);

      if (dbDepartmentProject is null)
      {
        return false;
      }

      dbDepartmentProject.DepartmentId = departmentId.HasValue ? departmentId.Value : dbDepartmentProject.DepartmentId;
      dbDepartmentProject.IsActive = departmentId.HasValue ? true : false;
      dbDepartmentProject.CreatedBy = _httpContextAccessor.HttpContext.GetUserId();

      await _provider.SaveAsync();

      return true;
    }
  }
}
