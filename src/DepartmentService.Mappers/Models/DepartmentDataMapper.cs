using System;
using System.Collections.Generic;
using System.Linq;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.Models.Broker.Models.Department;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models
{
  public class DepartmentDataMapper : IDepartmentDataMapper
  {
    private readonly IDepartmentUserDataMapper _departmentUserDataMapper;

    public DepartmentDataMapper(IDepartmentUserDataMapper departmentUserDataMapper)
    {
      _departmentUserDataMapper = departmentUserDataMapper;
    }

    public DepartmentData Map(DbDepartment dbDepartment, List<Guid> childIds)
    {
      return dbDepartment is null
        ? null
        : new DepartmentData(
          dbDepartment.Id,
          dbDepartment.Name,
          dbDepartment.ShortName,
          users: dbDepartment.Users?.Select(_departmentUserDataMapper.Map).ToList(),
          childDepartmentsIds: childIds);
    }
  }
}
