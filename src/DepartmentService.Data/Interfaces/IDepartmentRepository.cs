using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Filters;
using LT.DigitalOffice.Kernel.Attributes;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.DepartmentService.Data.Interfaces
{
  [AutoInject]
  public interface IDepartmentRepository
  {
    Task<Guid> CreateAsync(DbDepartment department);

    Task<List<DbDepartment>> GetAsync(List<Guid> departmentsIds, bool includeUsers = false);

    Task<DbDepartment> GetAsync(GetDepartmentFilter filter);

    Task<(List<DbDepartment>, int totalCount)> FindAsync(FindDepartmentFilter filter);

    Task<bool> EditAsync(DbDepartment department, JsonPatchDocument<DbDepartment> request);

    Task<List<DbDepartment>> SearchAsync(string text);

    Task<bool> DoesNameExistAsync(string name);
  }
}
