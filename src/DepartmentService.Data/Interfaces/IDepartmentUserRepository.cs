using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Requests.Company;

namespace LT.DigitalOffice.DepartmentService.Data.Interfaces
{
  [AutoInject]
  public interface IDepartmentUserRepository
  {
    Task<bool> CreateAsync(List<DbDepartmentUser> departmentsUsers);

    Task<DbDepartmentUser> GetAsync(Guid userId, bool includeDepartment);

    Task<(List<Guid> usersIds, int totalCount)> GetAsync(IGetDepartmentUsersRequest request);

    Task<List<DbDepartmentUser>> GetAsync(List<Guid> usersIds);

    Task RemoveAsync(IEnumerable<Guid> usersIds);

    Task<bool> RemoveAsync(Guid departmentId, IEnumerable<Guid> usersIds);

    Task<bool> ChangeDirectorAsync(Guid departmentId, Guid newDirectorId);
  }
}
