using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Models.Broker.Models.Position;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models
{
  public class PositionInfoMapper : IPositionInfoMapper
  {
    public PositionInfo Map(PositionData positionData)
    {
      if (positionData == null)
      {
        return null;
      }

      return new PositionInfo
      {
        Id = positionData.Id,
        Name = positionData.Name
      };
    }
  }
}
