using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Position;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models
{
  public class UserInfoMapper : IUserInfoMapper
  {
    public UserInfo Map(
      DbDepartmentUser dbDepartmentUser,
      UserData userData,
      ImageInfo image,
      PositionData userPosition)
    {
      if (dbDepartmentUser is null)
      {
        return null;
      }

      if (userData?.Id != dbDepartmentUser.UserId)
      {
        return null;
      }

      return new UserInfo
      {
        Id = userData.Id,
        FirstName = userData.FirstName,
        LastName = userData.LastName,
        MiddleName = userData.MiddleName,
        IsActive = dbDepartmentUser.IsActive,
        AvatarImage = image,
        Assignment = (DepartmentUserAssignment)dbDepartmentUser.Assignment,
        Role = (DepartmentUserRole)dbDepartmentUser.Role,
        Position = userPosition is null
          ? null
          : new()
          {
            Id = userPosition.Id,
            Name = userPosition.Name
          }
      };
    }
  }
}
