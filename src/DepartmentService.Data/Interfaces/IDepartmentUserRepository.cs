using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Requests.Department;

namespace LT.DigitalOffice.DepartmentService.Data.Interfaces
{
  [AutoInject]
  public interface IDepartmentUserRepository
  {
    Task<bool> CreateAsync(List<DbDepartmentUser> departmentsUsers);

    Task<List<Guid>> EditAsync(List<DbDepartmentUser> request);

    Task<bool> EditRoleAsync(List<Guid> usersIds, DepartmentUserRole role);

    Task<bool> EditAssignmentAsync(Guid departmentId, List<Guid> usersIds, DepartmentUserAssignment assignment);

    Task<List<DbDepartmentUser>> GetAsync(IGetDepartmentsUsersRequest request);

    Task<List<DbDepartmentUser>> GetAsync(List<Guid> usersIds, bool includeDepartments = false);

    Task<DbDepartmentUser> GetAsync(Guid userId, bool includeDepartment = false);

    Task<Guid?> RemoveAsync(Guid userId, Guid removedBy);

    Task RemoveAsync(Guid departmentId, List<Guid> usersIds = null);

    Task RemoveAsync(List<Guid> departmentIds);

    Task RemoveDirectorAsync(Guid departmentId);

    Task<bool> IsManagerAsync(Guid userId);
  }
}
