using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models
{
  public class DepartmentInfoMapper : IDepartmentInfoMapper
  {
    public DepartmentInfo Map(DbDepartment dbDepartment, DepartmentUserInfo director)
    {
      return dbDepartment is null
        ? null
        : new DepartmentInfo
        {
          Id = dbDepartment.Id,
          Name = dbDepartment.Name,
          Description = dbDepartment.Description,
          Director = director,
          IsActive = dbDepartment.IsActive,
          CountUsers = dbDepartment.Users.Count
        };
    }
  }
}
