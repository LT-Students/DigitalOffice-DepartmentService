using System;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.DepartmentService.Data.Interfaces
{
  [AutoInject]
  public interface IDepartmentProjectRepository
  {
    Task<Guid?> CreateAsync(DbDepartmentProject dbDepartmentProject);

    Task<DbDepartmentUser> GetAsync(Guid userId, bool includeDepartment = false);

    Task RemoveAsync(Guid projectId);
  }
}
