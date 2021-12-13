using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Models;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces
{
  [AutoInject]
  public interface IUserInfoMapper
  {
    UserInfo Map(UserData userData, ImageData imageData);
  }
}
