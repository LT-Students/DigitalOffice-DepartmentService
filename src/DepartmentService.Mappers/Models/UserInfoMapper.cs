using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Company;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models
{
  public class UserInfoMapper : IUserInfoMapper
  {
    private readonly IPositionInfoMapper _positionInfoMapper;

    public UserInfoMapper(IPositionInfoMapper positionInfoMapper)
    {
      _positionInfoMapper = positionInfoMapper;
    }

    public UserInfo Map(
      UserData userData,
      PositionData positionData,
      ImageInfo imageInfo,
      DbDepartmentUser dbDepartmentUser)
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
        Rate = userData.Rate,
        IsActive = userData.IsActive,
        CreatedAtUtc = dbDepartmentUser.CreatedAtUtc,
        ModifiedAtUtc = dbDepartmentUser.ModifiedAtUtc,
        AvatarImage = imageInfo,
        Role = (DepartmentUserRole)dbDepartmentUser.Role,
        Position = _positionInfoMapper.Map(positionData)
      };
    }
  }
}
