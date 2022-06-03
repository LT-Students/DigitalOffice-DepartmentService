using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Models.Department;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models
{
  public class DepartmentUserDataMapper : IDepartmentUserDataMapper
  {
    public DepartmentUserData Map(DbDepartmentUser dbDepartment)
    {
      return dbDepartment is null
        ? null
        : new DepartmentUserData(dbDepartment.UserId, (DepartmentUserRole)dbDepartment.Role);
    }
  }
}
