using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.Department.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Responses.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Filters;
using LT.DigitalOffice.DepartmentService.Models.Dto.Responses;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Position;
using LT.DigitalOffice.Models.Broker.Requests.Image;
using LT.DigitalOffice.Models.Broker.Requests.Position;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.Models.Broker.Requests.User;
using LT.DigitalOffice.Models.Broker.Responses.Image;
using LT.DigitalOffice.Models.Broker.Responses.Position;
using LT.DigitalOffice.Models.Broker.Responses.Project;
using LT.DigitalOffice.Models.Broker.Responses.User;
using MassTransit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace LT.DigitalOffice.DepartmentService.Business.Department
{
  public class GetDepartmentCommand : IGetDepartmentCommand
  {
    private readonly ILogger<GetDepartmentCommand> _logger;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IDepartmentResponseMapper _departmentResponseMapper;
    private readonly IUserInfoMapper _userInfoMapper;
    private readonly IProjectInfoMapper _projectInfoMapper;
    private readonly IRequestClient<IGetImagesRequest> _rcImages;
    private readonly IRequestClient<IGetUsersDataRequest> _rcGetUsersData;
    private readonly IRequestClient<IGetProjectsRequest> _rcGetProjects;
    private readonly IRequestClient<IGetPositionsRequest> _rcGetPositions;
    private readonly IConnectionMultiplexer _cache;
    private readonly IResponseCreater _responseCreator;

    private async Task<List<UserData>> GetUsersDatasAsync(IEnumerable<DbDepartmentUser> departmentUsers, List<string> errors)
    {
      if (departmentUsers == null || !departmentUsers.Any())
      {
        return null;
      }

      List<Guid> usersIds = departmentUsers.Select(x => x.UserId).ToList();

      RedisValue usersFromCache = await _cache.GetDatabase(Cache.Users).StringGetAsync(usersIds.GetRedisCacheHashCode());

      if (usersFromCache.HasValue)
      {
        _logger.LogInformation("UsersDatas were taken from the cache. Users ids: {usersIds}", string.Join(", ", usersIds));

        return JsonConvert.DeserializeObject<List<UserData>>(usersFromCache);
      }

      return await GetUsersDatasThroughBrokerAsync(usersIds, errors);
    }

    private async Task<List<UserData>> GetUsersDatasThroughBrokerAsync(List<Guid> usersIds, List<string> errors)
    {
      if (usersIds == null || !usersIds.Any())
      {
        return new();
      }

      string loggerMessage = $"Can not get users data for specific user ids:'{string.Join(",", usersIds)}'.";

      try
      {
        Response<IOperationResult<IGetUsersDataResponse>> response =
          await _rcGetUsersData.GetResponse<IOperationResult<IGetUsersDataResponse>>(
            IGetUsersDataRequest.CreateObj(usersIds));

        if (response.Message.IsSuccess)
        {
          _logger.LogInformation("UsersData were taken from the service. Users ids: {usersIds}", string.Join(", ", usersIds));

          return response.Message.Body.UsersData;
        }

        _logger.LogWarning(loggerMessage + "Reasons: {Errors}", string.Join("\n", response.Message.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, loggerMessage);
      }

      errors.Add("Can not get users data. Please try again later.");

      return null;
    }

    private async Task<List<ProjectData>> GetProjectsDatasAsync(IEnumerable<DbDepartmentProject> departmentProjects, List<string> errors)
    {
      if (departmentProjects == null || !departmentProjects.Any())
      {
        return null;
      }

      List<Guid> projectsIds = departmentProjects.Select(x => x.ProjectId).ToList();

      RedisValue usersFromCache = await _cache.GetDatabase(Cache.Projects).StringGetAsync(projectsIds.GetRedisCacheHashCode());

      if (usersFromCache.HasValue)
      {
        _logger.LogInformation("ProjectsDatas were taken from the cache. Projects ids: {projectsIds}", string.Join(", ", projectsIds));

        return JsonConvert.DeserializeObject<List<ProjectData>>(usersFromCache);
      }

      return await GetProjectsDatasThroughBrokerAsync(projectsIds, errors);
    }

    private async Task<List<ProjectData>> GetProjectsDatasThroughBrokerAsync(List<Guid> projectsIds, List<string> errors)
    {
      if (projectsIds == null || !projectsIds.Any())
      {
        return new();
      }

      string loggerMessage = $"Can not get projects data for specific projects id '{projectsIds}'.";

      try
      {
        Response<IOperationResult<IGetProjectsResponse>> response =
          await _rcGetProjects.GetResponse<IOperationResult<IGetProjectsResponse>>(
            IGetProjectsRequest.CreateObj(projectsIds));

        if (response.Message.IsSuccess)
        {
          return response.Message.Body.Projects;
        }

        _logger.LogWarning(loggerMessage + "Reasons: {Errors}", string.Join("\n", response.Message.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, loggerMessage);
      }

      errors.Add("Can not get projects data. Please try again later.");

      return new();
    }

    private async Task<List<ImageData>> GetUserImagesAsync(List<Guid> usersIds, List<string> errors)
    {
      if (usersIds == null || !usersIds.Any())
      {
        return null;
      }

      try
      {
        Response<IOperationResult<IGetImagesResponse>> response =
          await _rcImages.GetResponse<IOperationResult<IGetImagesResponse>>(
            IGetImagesRequest.CreateObj(usersIds, ImageSource.User));

        if (response.Message.IsSuccess && response.Message.Body.ImagesData != null)
        {
          return response.Message.Body.ImagesData;
        }

        _logger.LogWarning(
          "Errors while getting images by users ids: {UsersIds}.\nErrors: {Errors}",
          string.Join(", ", usersIds),
          string.Join('\n', response.Message.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(
          exc,
          "Errors while getting images by users ids: {UsersIds}.",
          string.Join(", ", usersIds));
      }

      errors.Add("Can not get images. Please try again later.");

      return null;
    }

    private async Task<List<PositionData>> GetPositionsAsync(List<Guid> usersIds, List<string> errors)
    {
      if (usersIds == null || !usersIds.Any())
      {
        return null;
      }

      try
      {
        Response<IOperationResult<IGetPositionsResponse>> response =
          await _rcGetPositions.GetResponse<IOperationResult<IGetPositionsResponse>>(
            IGetPositionsRequest.CreateObj(
            usersIds));

        if (response.Message.IsSuccess)
        {
          return response.Message.Body.Positions;
        }

        _logger.LogWarning(
          "Errors while getting positions of users ids {UserId}.\n Errors: {Errors}",
          string.Join(", ", usersIds),
          string.Join('\n', response.Message.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(
          exc,
          "Can not get positions of users ids {UserId}.",
          string.Join(", ", usersIds));
      }

      errors.Add("Can not get users positions. Please try again later.");

      return null;
    }

    public GetDepartmentCommand(
      IDepartmentRepository departmentRepository,
      IDepartmentResponseMapper departmentResponseMapper,
      IUserInfoMapper userInfoMapper,
      IProjectInfoMapper projectInfoMapper,
      IRequestClient<IGetImagesRequest> rcImages,
      IRequestClient<IGetUsersDataRequest> rcGetUsersData,
      IRequestClient<IGetProjectsRequest> rcGetProjects,
      IRequestClient<IGetPositionsRequest> rcGetPositions,
      IConnectionMultiplexer cache,
      ILogger<GetDepartmentCommand> logger,
      IResponseCreater responseCreator)
    {
      _logger = logger;
      _cache = cache;
      _rcImages = rcImages;
      _rcGetUsersData = rcGetUsersData;
      _rcGetProjects = rcGetProjects;
      _rcGetPositions = rcGetPositions;
      _departmentRepository = departmentRepository;
      _departmentResponseMapper = departmentResponseMapper;
      _projectInfoMapper = projectInfoMapper;
      _userInfoMapper = userInfoMapper;
      _responseCreator = responseCreator;
    }

    public async Task<OperationResultResponse<DepartmentResponse>> ExecuteAsync(GetDepartmentFilter filter)
    {
      OperationResultResponse<DepartmentResponse> response = new();
      DbDepartment dbDepartment = await _departmentRepository.GetAsync(filter);

      if (dbDepartment == null)
      {
        return _responseCreator.CreateFailureResponse<DepartmentResponse>(HttpStatusCode.NotFound);
      }

      List<ProjectData> projectData = await GetProjectsDatasAsync(dbDepartment.Projects, response.Errors);
      IEnumerable<ProjectInfo> projectInfo = projectData?.Select(_projectInfoMapper.Map);

      List<UserData> usersData = await GetUsersDatasAsync(dbDepartment.Users, response.Errors);
      IEnumerable<UserInfo> usersInfo = null;

      if (usersData != null && usersData.Any())
      {
        List<PositionData> positionsData = await GetPositionsAsync(
          usersData.Select(u => u.Id).Distinct().ToList(),
          response.Errors);

        List<ImageData> imagesData = await GetUserImagesAsync(
          usersData.Where(u => u.ImageId.HasValue).Select(u => u.ImageId.Value).Distinct().ToList(),
          response.Errors);

        usersInfo = usersData.Select(
          u =>
            _userInfoMapper.Map(
              u,
              dbDepartment.Users.FirstOrDefault(du => du.UserId == u.Id),
              imagesData.FirstOrDefault(i => i.ImageId == u.ImageId),
              positionsData.FirstOrDefault(p => p.Users.Select(u => u.UserId).Contains(u.Id))
          ));
      }

      response.Status = response.Errors.Any() ?
        OperationResultStatusType.PartialSuccess :
        OperationResultStatusType.FullSuccess;

      response.Body = _departmentResponseMapper.Map(dbDepartment, usersInfo, projectInfo);

      return response;
    }
  }
}
