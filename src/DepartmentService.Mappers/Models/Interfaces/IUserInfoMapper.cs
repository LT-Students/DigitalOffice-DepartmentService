using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Company;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces
{
  [AutoInject]
  public interface IUserInfoMapper
  {
    UserInfo Map(UserData userData, PositionData positionData, ImageInfo imageInfo, DbDepartmentUser dbDepartmentUser);
  }
}
