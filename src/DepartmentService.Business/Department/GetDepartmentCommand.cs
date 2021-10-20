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
using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.Models.Broker.Requests.Company;
using LT.DigitalOffice.Models.Broker.Requests.Image;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.Models.Broker.Requests.User;
using LT.DigitalOffice.Models.Broker.Responses.Company;
using LT.DigitalOffice.Models.Broker.Responses.Image;
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
    private readonly IImageInfoMapper _imageInfoMapper;
    private readonly IProjectInfoMapper _projectInfoMapper;
    private readonly IRequestClient<IGetImagesRequest> _rcImages;
    private readonly IRequestClient<IGetUsersDataRequest> _rcDepartmentUsers;
    private readonly IRequestClient<IGetProjectsRequest> _rcGetProjects;
    private readonly IRequestClient<IGetPositionsRequest> _rcGetPositions;
    private readonly IConnectionMultiplexer _cache;
    private readonly IResponseCreater _responseCreater;

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
          await _rcDepartmentUsers.GetResponse<IOperationResult<IGetUsersDataResponse>>(
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

    private async Task<List<ImageInfo>> GetUserAvatarsAsync(List<Guid> imageIds, List<string> errors)
    {
      if (imageIds == null || !imageIds.Any())
      {
        return null;
      }

      string logMessage = "Errors while getting images with ids: {Ids}. Errors: {Errors}";

      try
      {
        Response<IOperationResult<IGetImagesResponse>> response = await _rcImages.GetResponse<IOperationResult<IGetImagesResponse>>(
          IGetImagesRequest.CreateObj(imageIds, ImageSource.User));

        if (response.Message.IsSuccess && response.Message.Body.ImagesData != null)
        {
          return response.Message.Body.ImagesData.Select(_imageInfoMapper.Map).ToList();
        }
        else
        {
          _logger.LogWarning(
            logMessage,
            string.Join(", ", imageIds),
            string.Join('\n', response.Message.Errors));
        }
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, logMessage, string.Join(", ", imageIds));
      }

      errors.Add("Can not get images. Please try again later.");

      return null;
    }

    private async Task<List<PositionData>> GetPositionAsync(List<Guid> positionIds, List<string> errors)
    {
      if (positionIds == null || !positionIds.Any())
      {
        return new();
      }

      string logMessage = "Can not get position: {ids}.";

      try
      {
        Response<IOperationResult<IGetPositionsResponse>> response =
          await _rcGetPositions.GetResponse<IOperationResult<IGetPositionsResponse>>(
            IGetPositionsRequest.CreateObj(positionIds));

        if (response.Message.IsSuccess)
        {
          return response.Message.Body.Positions;
        }

        _logger.LogWarning(logMessage + "Reason: {Errors}", string.Join(", ", positionIds), string.Join("\n", response.Message.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, logMessage, string.Join(", ", positionIds));
      }

      errors.Add("Can not get positions. Please try again later.");

      return new();
    }

    public GetDepartmentCommand(
      IDepartmentRepository departmentRepository,
      IDepartmentResponseMapper departmentResponseMapper,
      IUserInfoMapper userInfoMapper,
      IImageInfoMapper imageInfoMapper,
      IProjectInfoMapper projectInfoMapper,
      IRequestClient<IGetImagesRequest> rcImages,
      IRequestClient<IGetUsersDataRequest> rcDepartmentUsers,
      IRequestClient<IGetProjectsRequest> rcGetProjects,
      IRequestClient<IGetPositionsRequest> rcGetPosition,
      IConnectionMultiplexer cache,
      ILogger<GetDepartmentCommand> logger,
      IResponseCreater responseCreater)
    {
      _logger = logger;
      _cache = cache;
      _rcImages = rcImages;
      _rcDepartmentUsers = rcDepartmentUsers;
      _rcGetProjects = rcGetProjects;
      _rcGetPositions = rcGetPosition;
      _departmentRepository = departmentRepository;
      _departmentResponseMapper = departmentResponseMapper;
      _imageInfoMapper = imageInfoMapper;
      _projectInfoMapper = projectInfoMapper;
      _userInfoMapper = userInfoMapper;
      _responseCreater = responseCreater;
    }

    public async Task<OperationResultResponse<DepartmentResponse>> ExecuteAsync(GetDepartmentFilter filter)
    {
      OperationResultResponse<DepartmentResponse> response = new();
      DbDepartment dbDepartment = await _departmentRepository.GetAsync(filter);

      if (dbDepartment == null)
      {
        return _responseCreater.CreateFailureResponse<DepartmentResponse>(HttpStatusCode.BadRequest);
      }

      List<ProjectData> projectData = await GetProjectsDatasAsync(dbDepartment.Projects, response.Errors);
      List<ProjectInfo> projectInfo = projectData?.Select(_projectInfoMapper.Map).ToList();

      List<UserData> usersDatas = await GetUsersDatasAsync(dbDepartment.Users, response.Errors);
      List<UserInfo> usersInfo = null;
      List<Guid> usersIds = dbDepartment.Users.Select(u => u.UserId).Distinct().ToList();

      if (usersDatas != null && usersDatas.Any())
      {
        List<PositionData> positions = await GetPositionAsync(usersIds, response.Errors);
        List<ImageInfo> imagesInfos =
          await GetUserAvatarsAsync(usersDatas.Where(u => u.ImageId.HasValue).Select(u => u.ImageId.Value).ToList(), response.Errors);

        usersInfo = dbDepartment.Users
          .Select(du =>
          {
            UserData user = usersDatas.FirstOrDefault(x => x.Id == du.UserId);

            return _userInfoMapper.Map(
              user,
              positions?.FirstOrDefault(d => d.UsersIds.Any(id => id == du.UserId)),
              imagesInfos?.FirstOrDefault(i => i.Id == user.ImageId),
              du);
          })
          .ToList();
      }

      response.Status = response.Errors.Any() ? OperationResultStatusType.PartialSuccess : OperationResultStatusType.FullSuccess;
      response.Body = _departmentResponseMapper.Map(dbDepartment, usersInfo, projectInfo);

      return response;
    }
  }
}
