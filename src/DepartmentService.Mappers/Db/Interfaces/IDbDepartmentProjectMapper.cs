using System;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces
{
  [AutoInject]
  public interface IDbDepartmentProjectMapper
  {
    DbDepartmentProject Map(Guid projectId, Guid departmentId, Guid CreatedBy);
    DbDepartmentProject Map(EditDepartmentProjectRequest request);
  }
}
