﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DigitalOffice.Models.Broker.Publishing;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.DepartmentUser;
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

    Task<Guid?> ActivateAsync(IActivateUserPublish request);

    Task<Guid?> MakeUserPendingAsync(Guid userId, Guid createdBy);

    Task<bool> EditRoleAsync(List<Guid> usersIds, DepartmentUserRole role);

    Task<bool> EditAssignmentAsync(Guid departmentId, List<Guid> usersIds, DepartmentUserAssignment assignment);

    Task<List<DbDepartmentUser>> GetAsync(Guid departmentId, FindDepartmentUsersFilter filter, CancellationToken cancellationToken = default);

    Task<List<DbDepartmentUser>> GetAsync(IGetDepartmentsUsersRequest request);

    Task<List<DbDepartmentUser>> GetAsync(List<Guid> usersIds, bool includeDepartments = false);

    Task<DbDepartmentUser> GetAsync(Guid userId, bool includeDepartment = false);

    Task<Guid?> RemoveAsync(Guid userId, Guid removedBy);

    Task RemoveAsync(Guid departmentId, List<Guid> usersIds = null);

    Task RemoveAsync(List<Guid> departmentIds);

    Task RemoveDirectorAsync(Guid departmentId);
  }
}
