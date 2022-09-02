using System;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Publishing.Subscriber.Department;

namespace LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces
{
  [AutoInject]
  public interface IDbDepartmentUserMapper
  {
    DbDepartmentUser Map(CreateUserRequest request, Guid departmentId);

    DbDepartmentUser Map(ICreateDepartmentUserPublish request);
  }
}
