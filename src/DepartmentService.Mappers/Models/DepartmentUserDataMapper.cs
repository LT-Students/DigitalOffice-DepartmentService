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
      if (dbDepartment is null)
      {
        return null;
      }

      return new DepartmentUserData(dbDepartment.UserId, (DepartmentUserRole)dbDepartment.Role);
    }
  }
}
