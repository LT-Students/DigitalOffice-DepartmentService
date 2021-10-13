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
    Task<bool> AddAsync(DbDepartmentUser departmentUser);

    Task<bool> AddAsync(List<DbDepartmentUser> departmentUsers);

    Task<DbDepartmentUser> GetAsync(Guid userId, bool includeDepartment);

    List<Guid> Get(IGetDepartmentUsersRequest request, out int totalCount);

    Task<List<DbDepartmentUser>> GetAsync(List<Guid> userIds);

    void RemoveAsync(Guid userId, Guid removedBy);

    void RemoveAsync(List<Guid> usersIds, Guid removedBy);

    Task<bool> IsDepartmentDirectorAsync(Guid departmentId, Guid userId);

    Task<bool> ChangeDirectorAsync(Guid departmentId, Guid newDirectorId);
  }
}
