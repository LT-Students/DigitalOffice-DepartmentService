using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Models.Broker.Models.Position;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models
{
  public class DepartmentUserInfoMapper : IDepartmentUserInfoMapper
  {
    private readonly IPositionInfoMapper _positionInfoMapper;

    public DepartmentUserInfoMapper(
      IPositionInfoMapper positionInfoMapper)
    {
      _positionInfoMapper = positionInfoMapper;
    }

    public DepartmentUserInfo Map(
      UserInfo userInfo,
      DbDepartmentUser dbDepartmentUser,
      PositionData positionData)
    {
      if (userInfo == null)
      {
        return null;
      }

      return new DepartmentUserInfo
      {
        User = userInfo,
        IsActive = dbDepartmentUser.IsActive,
        Role = (DepartmentUserRole)(dbDepartmentUser?.Role),
        CreatedAtUtc = dbDepartmentUser?.CreatedAtUtc,
        LeftAtUtc = dbDepartmentUser?.LeftAtUtc,
        Position = _positionInfoMapper.Map(positionData)
      };
    }
  }
}
