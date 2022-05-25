using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department.Filters;
using LT.DigitalOffice.Kernel.Attributes;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.DepartmentService.Data.Interfaces
{
  [AutoInject]
  public interface IDepartmentRepository
  {
    Task<Guid?> CreateAsync(DbDepartment dbDepartment);

    Task<DbDepartment> GetAsync(GetDepartmentFilter filter);

    Task<List<DbDepartment>> GetAsync(
      List<Guid> departmentsIds = null,
      List<Guid> usersIds = null,
      List<Guid> projectsIds = null);

    Task<(List<DbDepartment> dbDepartments, int totalCount)> FindAsync(FindDepartmentFilter filter);

    Task<bool> EditAsync(Guid departmentId, JsonPatchDocument<DbDepartment> request);

    Task<List<DbDepartment>> SearchAsync(string text);

    Task<bool> NameExistAsync(string name);

    Task<bool> ShortNameExistAsync(string shortName);

    Task<bool> ExistAsync(Guid departmentId);
  }
}
