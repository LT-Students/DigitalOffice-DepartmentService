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

    public DepartmentData Map(DbDepartment dbDepartment)
    {
      return dbDepartment is null
        ? null
        : new DepartmentData(
          dbDepartment.Id,
          dbDepartment.Name,
          dbDepartment.ShortName,
          projectsIds: dbDepartment.Projects?.Select(dp => dp.ProjectId).ToList(),
          users: dbDepartment.Users.Select(_departmentUserDataMapper.Map).ToList());
    }
  }
}
