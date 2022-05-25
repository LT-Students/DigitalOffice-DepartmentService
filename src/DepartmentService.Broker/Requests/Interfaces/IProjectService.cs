using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Models.Project;

namespace LT.DigitalOffice.DepartmentService.Broker.Requests.Interfaces
{
  [AutoInject]
  public interface IProjectService
  {
    Task<List<ProjectData>> GetProjectsAsync(List<Guid> projectsIds, List<string> errors);
  }
}
