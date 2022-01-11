using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Models.Broker.Models;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models
{
  public class UserInfoMapper : IUserInfoMapper
  {
    private readonly IImageInfoMapper _imageInfoMapper;

    public UserInfoMapper(IImageInfoMapper imageInfoMapper)
    {
      _imageInfoMapper = imageInfoMapper;
    }

    public UserInfo Map(UserData userData, ImageData imageData)
    {
      if (userData is null)
      {
        return null;
      }

      return new()
      {
        Id = userData.Id,
        FirstName = userData.FirstName,
        LastName = userData.LastName,
        MiddleName = userData.MiddleName,
        Avatar = _imageInfoMapper.Map(imageData)
      };
    }
  }
}
