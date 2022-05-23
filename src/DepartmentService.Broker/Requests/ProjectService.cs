using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Broker.Requests.Interfaces;
using LT.DigitalOffice.Kernel.BrokerSupport.Helpers;
using LT.DigitalOffice.Kernel.RedisSupport.Constants;
using LT.DigitalOffice.Kernel.RedisSupport.Extensions;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using LT.DigitalOffice.Models.Broker.Models.Project;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.Models.Broker.Responses.Project;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.DepartmentService.Broker.Requests
{
  public class ProjectService : IProjectService
  {
    private readonly IRequestClient<IGetProjectsRequest> _rcGetProjects;
    private readonly ILogger<ProjectService> _logger;
    private readonly IGlobalCacheRepository _globalCache;

    public ProjectService(
      IRequestClient<IGetProjectsRequest> rcGetProjects,
      ILogger<ProjectService> logger,
      IGlobalCacheRepository globalCache)
    {
      _rcGetProjects = rcGetProjects;
      _logger = logger;
      _globalCache = globalCache;
    }


    public async Task<List<ProjectData>> GetProjectsAsync(List<Guid> projectsIds, List<string> errors)
    {
      (List<ProjectData> projects, int _) = await _globalCache
        .GetAsync<(List<ProjectData>, int)>(Cache.Projects, projectsIds.GetRedisCacheHashCode());

      if (projects is not null)
      {
        _logger.LogInformation(
          "Project for projects ids '{ProjectsIds}' were taken from cache.",
          string.Join(", ", projectsIds));
      }
      else
      {
        projects = (await _rcGetProjects.ProcessRequest<IGetProjectsRequest, IGetProjectsResponse>(
            IGetProjectsRequest.CreateObj(projectsIds: projectsIds),
            errors,
            _logger))
          ?.Projects;
      }

      return projects;
    }
  }
}
