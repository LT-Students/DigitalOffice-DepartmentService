using System;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces
{
  [AutoInject]
  public interface IDbDepartmentUserMapper
  {
    DbDepartmentUser Map(CreateUserRequest request, Guid departmentId);
  }
}
