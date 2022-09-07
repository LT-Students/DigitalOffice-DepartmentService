using System;
using System.Collections.Generic;
using System.Threading;
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

    Task<DbDepartment> GetAsync(GetDepartmentFilter filter, CancellationToken cancellationToken = default);

    Task<List<DbDepartment>> GetAsync(
      List<Guid> departmentsIds = null,
      List<Guid> usersIds = null);

    Task<(List<DbDepartment> dbDepartments, int totalCount)> FindAsync(FindDepartmentFilter filter, CancellationToken cancellationToken = default);

    Task<bool> EditAsync(Guid departmentId, JsonPatchDocument<DbDepartment> request);

    Task<List<DbDepartment>> SearchAsync(string text);

    Task<bool> NameExistAsync(string name);

    Task<bool> ShortNameExistAsync(string shortName);

    Task<bool> ExistAsync(Guid departmentId);

    Task<List<Tuple<Guid, string, string, Guid?>>> GetDepartmentsTreeAsync(FindDepartmentFilter filter);

    Task RemoveAsync(List<Guid> departmentsIds);
  }
}
