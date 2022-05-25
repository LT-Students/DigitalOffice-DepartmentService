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

    Task<bool> EditAsync(Guid projectId, Guid? departmentId);
  }
}
