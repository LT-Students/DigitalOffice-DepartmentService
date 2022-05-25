using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces
{
  [AutoInject]
  public interface IDepartmentUserInfoMapper
  {
    DepartmentUserInfo Map(
      UserInfo userInfo,
      DbDepartmentUser dbDepartmentUser);
  }
}
