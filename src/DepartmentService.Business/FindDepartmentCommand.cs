using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Filters;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Kernel.Validators.Interfaces;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.Models.Broker.Requests.Company;
using LT.DigitalOffice.Models.Broker.Requests.Image;
using LT.DigitalOffice.Models.Broker.Requests.User;
using LT.DigitalOffice.Models.Broker.Responses.Company;
using LT.DigitalOffice.Models.Broker.Responses.Image;
using LT.DigitalOffice.Models.Broker.Responses.User;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace LT.DigitalOffice.DepartmentService.Business
{
  public class FindDepartmentsCommand : IFindDepartmentsCommand
  {
    private readonly IDepartmentRepository _repository;
    private readonly IDepartmentInfoMapper _departmentMapper;
    private readonly IDepartmentUserInfoMapper _userMapper;
    private readonly IRequestClient<IGetUsersDataRequest> _rcGetUsersData;
    private readonly IRequestClient<IGetImagesRequest> _rcGetImages;
    private readonly IRequestClient<IGetPositionsRequest> _rcGetPositions;
    private readonly ILogger<FindDepartmentsCommand> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBaseFindFilterValidator _baseFindValidator;
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

      return await GetUsersDataFromBrokerAsync(usersIds, errors);
    }

    private async Task<List<UserData>> GetUsersDataFromBrokerAsync(List<Guid> usersIds, List<string> errors)
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

      errors.Add(message);

      return null;
    }

    private async Task<List<ImageData>> GetImagesAsync(List<Guid> imagesIds, List<string> errors)
    {
      if (imagesIds == null || !imagesIds.Any())
      {
        return new();
      }

      string logMessage = "Can not get images: {ids}.";
      string errorMessage = "Can not get images. Please try again later.";

      try
      {
        Response<IOperationResult<IGetImagesResponse>> response = await _rcGetImages.GetResponse<IOperationResult<IGetImagesResponse>>(
          IGetImagesRequest.CreateObj(imagesIds, ImageSource.User));

        if (response.Message.IsSuccess)
        {
          return response.Message.Body.ImagesData;
        }

        _logger.LogWarning(logMessage + "Reason: {Errors}", string.Join(", ", imagesIds), string.Join("\n", response.Message.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, logMessage, string.Join(", ", imagesIds));
      }

      errors.Add(errorMessage);

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

    public FindDepartmentsCommand(
      IBaseFindFilterValidator baseFindValidator,
      IDepartmentRepository repository,
      IDepartmentInfoMapper departmentMapper,
      IDepartmentUserInfoMapper userMapper,
      IRequestClient<IGetUsersDataRequest> rcGetUsersData,
      IRequestClient<IGetImagesRequest> rcGetImages,
      IRequestClient<IGetPositionsRequest> rcGetPosition,
      ILogger<FindDepartmentsCommand> logger,
      IHttpContextAccessor httpContextAccessor,
      IConnectionMultiplexer cache)
    {
      _baseFindValidator = baseFindValidator;
      _repository = repository;
      _departmentMapper = departmentMapper;
      _userMapper = userMapper;
      _rcGetUsersData = rcGetUsersData;
      _rcGetImages = rcGetImages;
      _rcGetPositions = rcGetPosition;
      _logger = logger;
      _httpContextAccessor = httpContextAccessor;
      _cache = cache;
    }

    public async Task<FindResultResponse<DepartmentInfo>> ExecuteAsync(FindDepartmentFilter filter)
    {

      FindResultResponse<DepartmentInfo> response = new(body: new());

      (List<DbDepartment> dbDepartments, int totalCount) = await _repository.FindAsync(filter);

      if (!_baseFindValidator.ValidateCustom(filter, out List<string> errors))
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        response.Status = OperationResultStatusType.Failed;
        response.Errors = errors;
        return response;
      }

      Dictionary<Guid, Guid> departmentsDirectors =
        dbDepartments
          .SelectMany(d => d.Users
          .Where(u => u.Role == (int)DepartmentUserRole.Director))
          .ToDictionary(d => d.DepartmentId, d => d.UserId);
      List<UserData> usersData = await GetUsersDataAsync(
        departmentsDirectors.Values.ToList(),
        response.Errors);

      List<Guid> positionIds = new();
      List<PositionData> positionData = await GetPositionAsync(positionIds, response.Errors);

      List<ImageData> images = await GetImagesAsync(
        usersData.Where(d => d.ImageId.HasValue).Select(d => d.ImageId.Value).ToList(),
        response.Errors);

      foreach (DbDepartment department in dbDepartments)
      {
        UserInfo director = null;
        if (departmentsDirectors.ContainsKey(department.Id) && usersData.Any())
        {
          UserData directorUserData = usersData.FirstOrDefault(x => x.Id == departmentsDirectors[department.Id]);

          PositionData positionUser = positionData.FirstOrDefault(x => x.Id == directorUserData.Id);

          director = directorUserData == null ? null :
            _userMapper.Map(directorUserData, positionUser, images.FirstOrDefault(i => i.ImageId == directorUserData.ImageId));
        }

        response.Body.Add(_departmentMapper.Map(department, director));
      }

      response.Status = response.Errors.Any() ? OperationResultStatusType.PartialSuccess : OperationResultStatusType.FullSuccess;

      return response;
    }
  }
}
