using System.Linq;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.Models.Broker.Models.Department;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models
{
  public class DepartmentDataMapper : IDepartmentDataMapper
  {
    public DepartmentData Map(DbDepartment dbDepartment)
    {
      return dbDepartment is null
        ? null
        : new DepartmentData(
          dbDepartment.Id,
          dbDepartment.Name,
          directorUserId: dbDepartment.Users
            ?.FirstOrDefault(du => du.Role == (int)DepartmentUserRole.Manager)
            ?.UserId,
          projectsIds: dbDepartment.Projects?.Select(dp => dp.ProjectId).ToList(),
          usersIds: dbDepartment.Users?.Select(u => u.UserId).ToList());
    }
  }
}
