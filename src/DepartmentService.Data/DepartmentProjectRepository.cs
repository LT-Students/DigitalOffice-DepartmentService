using System;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Provider;
using LT.DigitalOffice.DepartmentService.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace LT.DigitalOffice.DepartmentService.Data
{
  public class DepartmentProjectRepository : IDepartmentProjectRepository
  {
    private readonly IDataProvider _provider;

    public DepartmentProjectRepository(
      IDataProvider provider)
    {
      _provider = provider;
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

    public async Task RemoveAsync(Guid projectId, Guid removedBy)
    {
      DbDepartmentProject dbDepartmentProject = await _provider.DepartmentsProjects
        .FirstOrDefaultAsync(dp => dp.ProjectId == projectId && dp.IsActive);

      if (dbDepartmentProject != null)
      {
        dbDepartmentProject.IsActive = false;
        dbDepartmentProject.ModifiedAtUtc = DateTime.UtcNow;
        dbDepartmentProject.ModifiedBy = removedBy;
        await _provider.SaveAsync();
      }
    }
  }
}
