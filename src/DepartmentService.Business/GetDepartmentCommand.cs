using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Responses.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Filters;
using LT.DigitalOffice.DepartmentService.Models.Dto.Responses;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Extensions;
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

namespace LT.DigitalOffice.DepartmentService.Business
{
  public class GetDepartmentCommand : IGetDepartmentCommand
  {
    private readonly ILogger<GetDepartmentCommand> _logger;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IDepartmentResponseMapper _departmentResponseMapper;
    private readonly IRequestClient<IGetImagesRequest> _rcImages;
    private readonly IRequestClient<IGetUsersDataRequest> _rcDepartmentUsers;
    private readonly IRequestClient<IGetProjectsRequest> _rcGetProjects;
    private readonly IRequestClient<IGetPositionsRequest> _rcGetPositions;
    private readonly IConnectionMultiplexer _cache;

    private async Task<List<UserData>> GetUsersDataAsync(List<Guid> usersIds, List<string> errors)
    {
      if (usersIds == null || !usersIds.Any())
      {
        return new();
      }

      RedisValue valueFromCache = await _cache.GetDatabase(Cache.Users).StringGetAsync(usersIds.GetRedisCacheHashCode());

      if (valueFromCache.HasValue)
      {
        _logger.LogInformation("UsersData were taken from the cache. Users ids: {usersIds}", string.Join(", ", usersIds));

        return JsonConvert.DeserializeObject<List<UserData>>(valueFromCache.ToString());
      }

      return await GetUsersDataThroughBrokerAsync(usersIds, errors);
    }

    private async Task<List<UserData>> GetUsersDataThroughBrokerAsync(List<Guid> usersIds, List<string> errors)
    {
      if (usersIds == null || !usersIds.Any())
      {
        return new();
      }

      string message = "Can not get users data. Please try again later.";
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

      errors.Add(message);

      return null;
    }

    private async Task<List<ProjectData>> GetProjectsDatasAsync(Guid departmentId, List<string> errors)
    {
      RedisValue projectsFromCache = await _cache.GetDatabase(Cache.Projects).StringGetAsync(departmentId.GetRedisCacheHashCode().ToString());

      if (projectsFromCache.HasValue)
      {
        (List<ProjectData> projects, int _) = JsonConvert.DeserializeObject<(List<ProjectData>, int)>(projectsFromCache);

        return projects;
      }

      return await GetProjectsDatasThroughBroker(departmentId, errors);
    }

    private async Task<List<ProjectData>> GetProjectsDatasThroughBroker(Guid departmentId, List<string> errors)
    {
      string message = "Can not get projects data. Please try again later.";
      string loggerMessage = $"Can not get projects data for specific department id '{departmentId}'.";

      try
      {
        Response<IOperationResult<IGetProjectsResponse>> response =
          await _rcGetProjects.GetResponse<IOperationResult<IGetProjectsResponse>>(
            IGetProjectsRequest.CreateObj(departmentId: departmentId));

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

      errors.Add(message);

      return new();
    }

    private async Task<List<ImageData>> GetUsersImageAsync(List<Guid> imagesIds, List<string> errors)
    {
      if (imagesIds == null || !imagesIds.Any())
      {
        return new();
      }

      string message = "Can not get users avatar. Please try again later.";
      string loggerMessage = $"Can not get users avatar by specific image ids '{string.Join(",", imagesIds)}.";

      try
      {
        Response<IOperationResult<IGetImagesResponse>> response =
          await _rcImages.GetResponse<IOperationResult<IGetImagesResponse>>(
            IGetImagesRequest.CreateObj(imagesIds, ImageSource.User));

        if (response.Message.IsSuccess)
        {
          return response.Message.Body.ImagesData;
        }

        _logger.LogWarning(loggerMessage + "Reasons: {Errors}", string.Join("\n", response.Message.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, loggerMessage);
      }

      errors.Add(message);

      return new();
    }

    private async Task<List<PositionData>> GetPositionAsync(List<Guid> positionIds, List<string> errors)
    {
      if (positionIds == null || !positionIds.Any())
      {
        return new();
      }

      string logMessage = "Can not get position: {ids}.";
      string errorMessage = "Can not get positions. Please try again later.";

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

      errors.Add(errorMessage);

      return new();
    }

    public GetDepartmentCommand(
      IDepartmentRepository departmentRepository,
      IDepartmentResponseMapper departmentResponseMapper,
      IRequestClient<IGetImagesRequest> rcImages,
      IRequestClient<IGetUsersDataRequest> rcDepartmentUsers,
      IRequestClient<IGetProjectsRequest> rcGetProjects,
      IRequestClient<IGetPositionsRequest> rcGetPosition,
      IConnectionMultiplexer cache,
      ILogger<GetDepartmentCommand> logger)
    {
      _logger = logger;
      _cache = cache;
      _rcImages = rcImages;
      _rcDepartmentUsers = rcDepartmentUsers;
      _rcGetProjects = rcGetProjects;
      _rcGetPositions = rcGetPosition;
      _departmentRepository = departmentRepository;
      _departmentResponseMapper = departmentResponseMapper;
    }

    public async Task<OperationResultResponse<DepartmentResponse>> ExecuteAsync(GetDepartmentFilter filter)
    {
      List<string> errors = new();

      DbDepartment dbDepartment = await _departmentRepository.GetAsync(filter);

      Guid? directorId = dbDepartment.Users.FirstOrDefault(u => u.Role == (int)DepartmentUserRole.Director)?.Id;

      List<Guid> userIds = new();
      if (filter.IsIncludeUsers)
      {
        userIds.AddRange(dbDepartment.Users.Select(u => u.UserId).ToList());
      }
      else if (directorId.HasValue)
      {
        userIds.Add(directorId.Value);
      }

      List<PositionData> positionData = await GetPositionAsync(userIds, errors);

      List<UserData> usersData = null;
      List<ImageData> userImages = null;
      if (directorId.HasValue || filter.IsIncludeUsers)
      {
        usersData = await GetUsersDataAsync(userIds, errors);
        userImages = await GetUsersImageAsync(usersData.Where(
          us => us.ImageId.HasValue).Select(us => us.ImageId.Value).ToList(), errors);
      }

      List<ProjectData> projectsDatas = null;
      if (filter.IsIncludeProjects)
      {
        projectsDatas = await GetProjectsDatasAsync(dbDepartment.Id, errors);
      }

      return new OperationResultResponse<DepartmentResponse>
      {
        Body = _departmentResponseMapper.Map(dbDepartment, usersData, positionData, userImages, projectsDatas, filter),
        Status = errors.Any() ? OperationResultStatusType.PartialSuccess : OperationResultStatusType.FullSuccess,
        Errors = errors
      };
    }
  }
}
