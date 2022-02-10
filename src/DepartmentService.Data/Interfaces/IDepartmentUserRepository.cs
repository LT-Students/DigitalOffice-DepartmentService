using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Requests.Department;

namespace LT.DigitalOffice.DepartmentService.Data.Interfaces
{
  [AutoInject]
  public interface IDepartmentUserRepository
  {
    Task<bool> CreateAsync(List<DbDepartmentUser> departmentsUsers);

    Task<Guid?> CreateAsync(DbDepartmentUser dbDepartmentUser);

    Task<(List<Guid> usersIds, int totalCount)> GetAsync(IGetDepartmentUsersRequest request);

    Task<List<DbDepartmentUser>> GetAsync(List<Guid> usersIds, bool includeDepartments = false);

    Task<DbDepartmentUser> GetAsync(Guid userId, bool includeDepartment = false);

    Task<List<Guid>> RemoveAsync(List<Guid> usersIds);

    Task<Guid?> RemoveAsync(Guid userId, Guid removedBy);

    Task<bool> RemoveAsync(Guid departmentId, IEnumerable<Guid> usersIds);

    Task<bool> ChangeDirectorAsync(Guid departmentId, Guid newDirectorId);
  }
}
