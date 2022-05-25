using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Models.Broker.Models;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models
{
  public class UserInfoMapper : IUserInfoMapper
  {
    public UserInfo Map(UserData userData, ImageInfo imageInfo)
    {
      return userData is null
        ? null
        : new()
        {
          Id = userData.Id,
          FirstName = userData.FirstName,
          LastName = userData.LastName,
          MiddleName = userData.MiddleName,
          Avatar = imageInfo
        };
    }
  }
}
