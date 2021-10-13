using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Models.Broker.Models;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models
{
  public class ProjectInfoMapper : IProjectInfoMapper
  {
    public ProjectInfo Map(ProjectData projectData)
    {
      if (projectData == null)
      {
        return null;
      }

      return new ProjectInfo
      {
        Id = projectData.Id,
        Name = projectData.Name,
        Status = projectData.Status,
        ShortName = projectData.ShortName,
        ShortDescription = projectData.ShortDescription
      };
    }
  }
}
