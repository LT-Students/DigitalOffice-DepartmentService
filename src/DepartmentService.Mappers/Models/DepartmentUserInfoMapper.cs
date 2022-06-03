using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Models.Broker.Enums;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models
{
  public class DepartmentUserInfoMapper : IDepartmentUserInfoMapper
  {
    public DepartmentUserInfo Map(
      UserInfo userInfo,
      DbDepartmentUser dbDepartmentUser)
    {
      return userInfo is null || dbDepartmentUser is null
        ? null
        : new DepartmentUserInfo
        {
          User = userInfo,
          IsActive = dbDepartmentUser.IsActive,
          Role = (DepartmentUserRole)dbDepartmentUser.Role,
          Assignment = (DepartmentUserAssignment)dbDepartmentUser.Assignment
        };
    }
  }
}
