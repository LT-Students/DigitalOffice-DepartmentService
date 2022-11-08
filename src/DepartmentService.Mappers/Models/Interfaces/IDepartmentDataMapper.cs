using System;
using System.Collections.Generic;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Models.Department;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces
{
  [AutoInject]
  public interface IDepartmentDataMapper
  {
    DepartmentData Map(DbDepartment dbDepartment, List<Guid> childrenIds);
  }
}
