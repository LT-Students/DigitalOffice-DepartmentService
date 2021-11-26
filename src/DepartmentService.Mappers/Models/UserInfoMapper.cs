using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Position;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models
{
  public class UserInfoMapper : IUserInfoMapper
  {
    private readonly IImageInfoMapper _imageInfoMapper;
    private readonly IPositionInfoMapper _positionInfoMapper;

    public UserInfoMapper(
      IImageInfoMapper imageInfoMapper,
      IPositionInfoMapper positionInfoMapper)
    {
      _imageInfoMapper = imageInfoMapper;
      _positionInfoMapper = positionInfoMapper;
    }

    public UserInfo Map(
      UserData userData,
      DbDepartmentUser dbDepartmentUser,
      ImageData imageData,
      PositionData positionData)
    {
      if (userData == null)
      {
        return null;
      }

      return new UserInfo
      {
        Id = userData.Id,
        FirstName = userData.FirstName,
        LastName = userData.LastName,
        MiddleName = userData.MiddleName,
        IsActive = userData.IsActive,
        Role = (DepartmentUserRole)dbDepartmentUser.Role,
        CreatedAtUtc = dbDepartmentUser.CreatedAtUtc,
        LeftAtUtc = dbDepartmentUser.LeftAtUtc,
        AvatarImage = _imageInfoMapper.Map(imageData),
        Position = _positionInfoMapper.Map(positionData)
      };
    }
  }
}
