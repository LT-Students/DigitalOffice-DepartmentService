using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Company;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models
{
  public class DepartmentUserInfoMapper : IDepartmentUserInfoMapper
  {
    private readonly IPositionInfoMapper _positionInfoMapper;
    private readonly IImageInfoMapper _imageMapper;

    public DepartmentUserInfoMapper(
      IPositionInfoMapper positionInfoMapper,
      IImageInfoMapper imageMapper)
    {
      _positionInfoMapper = positionInfoMapper;
      _imageMapper = imageMapper;
    }

    public UserInfo Map(UserData userData, PositionData positionData, ImageData image)
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
        Image = image != null ? _imageMapper.Map(image) : null,
        Position = positionData != null ? _positionInfoMapper.Map(positionData) : null
      };
    }
  }
}
