using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.Department.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Filters;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Kernel.Validators.Interfaces;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Position;
using LT.DigitalOffice.Models.Broker.Requests.Image;
using LT.DigitalOffice.Models.Broker.Requests.Position;
using LT.DigitalOffice.Models.Broker.Requests.User;
using LT.DigitalOffice.Models.Broker.Responses.Image;
using LT.DigitalOffice.Models.Broker.Responses.Position;
using LT.DigitalOffice.Models.Broker.Responses.User;
using MassTransit;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace LT.DigitalOffice.DepartmentService.Business.Department
{
  public class FindDepartmentsCommand : IFindDepartmentsCommand
  {
    private readonly IDepartmentRepository _repository;
    private readonly IDepartmentInfoMapper _departmentMapper;
    private readonly IUserInfoMapper _userMapper;
    private readonly IDepartmentUserInfoMapper _departmentUserMapper;
    private readonly IRequestClient<IGetUsersDataRequest> _rcGetUsersData;
    private readonly IRequestClient<IGetImagesRequest> _rcGetImages;
    private readonly IRequestClient<IGetPositionsRequest> _rcGetPositions;
    private readonly ILogger<FindDepartmentsCommand> _logger;
    private readonly IBaseFindFilterValidator _baseFindValidator;
    private readonly IResponseCreator _responseCreator;

    private async Task<List<UserData>> GetUsersDataAsync(List<Guid> usersIds, List<string> errors)
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
          return response.Message.Body.UsersData;
        }

        _logger.LogWarning(
          "Error while getting users data by users ids: {UsersIds}.\nReason: {Errors}",
          string.Join(", ", usersIds),
          string.Join('\n', response.Message.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(
          exc,
          "Can not get users data by users ids: {UsersIds}.",
          string.Join(", ", usersIds));
      }

      errors.Add("Can not get users data. Please try again later.");

      return null;
    }

    private async Task<List<ImageData>> GetImagesAsync(List<Guid> imagesIds, List<string> errors)
    {
      if (imagesIds == null || !imagesIds.Any())
      {
        return new();
      }

      try
      {
        Response<IOperationResult<IGetImagesResponse>> response =
          await _rcGetImages.GetResponse<IOperationResult<IGetImagesResponse>>(
            IGetImagesRequest.CreateObj(imagesIds, ImageSource.User));

        if (response.Message.IsSuccess)
        {
          return response.Message.Body.ImagesData;
        }

        _logger.LogWarning(
          "Error while getting images ids: {ImagesIds}.\nReason: {Errors}",
          string.Join(", ", imagesIds),
          string.Join('\n', response.Message.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(
          exc,
          "Can not get images ids: {ImagesIds}.",
          string.Join(", ", imagesIds));
      }

      errors.Add("Can not get users images. Please try again later.");

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

    public FindDepartmentsCommand(
      IBaseFindFilterValidator baseFindValidator,
      IDepartmentRepository repository,
      IDepartmentInfoMapper departmentMapper,
      IUserInfoMapper UserMapper,
      IDepartmentUserInfoMapper departmentUserMapper,
      IRequestClient<IGetUsersDataRequest> rcGetUsersData,
      IRequestClient<IGetImagesRequest> rcGetImages,
      IRequestClient<IGetPositionsRequest> rcGetPositions,
      ILogger<FindDepartmentsCommand> logger,
      IResponseCreator responseCreator)
    {
      _baseFindValidator = baseFindValidator;
      _repository = repository;
      _departmentMapper = departmentMapper;
      _userMapper = UserMapper;
      _departmentUserMapper = departmentUserMapper;
      _rcGetUsersData = rcGetUsersData;
      _rcGetImages = rcGetImages;
      _rcGetPositions = rcGetPositions;
      _logger = logger;
      _responseCreator = responseCreator;
    }

    public async Task<FindResultResponse<DepartmentInfo>> ExecuteAsync(FindDepartmentFilter filter)
    {
      if (!_baseFindValidator.ValidateCustom(filter, out List<string> errors))
      {
        return _responseCreator.CreateFailureFindResponse<DepartmentInfo>(
          HttpStatusCode.BadRequest, errors);
      }

      FindResultResponse<DepartmentInfo> response = new() { Body = new() };

      (List<DbDepartment> dbDepartments, int totalCount) = await _repository.FindAsync(filter);

      if (dbDepartments == null || !dbDepartments.Any())
      {
        return response;
      }

      Dictionary<Guid, Guid> departmentsDirectors =
        dbDepartments
          .SelectMany(d => d.Users.Where(u => u.Role == (int)DepartmentUserRole.Director))
          .ToDictionary(d => d.DepartmentId, d => d.UserId);

      List<UserData> usersData = await GetUsersDataAsync(
        departmentsDirectors.Values.ToList(),
        response.Errors);

      List<ImageData> imagesData = await GetImagesAsync(
        usersData?.Where(ud => ud.ImageId.HasValue)?.Select(ud => ud.ImageId.Value).ToList(),
        response.Errors);

      List<PositionData> positionsData = await GetPositionsAsync(
        usersData?.Select(u => u.Id).ToList(),
        response.Errors);

      UserData userData = null;

      foreach (DbDepartment dbDepartment in dbDepartments)
      {
        userData = departmentsDirectors.ContainsKey(dbDepartment.Id)
          ? usersData.FirstOrDefault(u => u.Id == departmentsDirectors[dbDepartment.Id])
          : null;

        response.Body.Add(
          _departmentMapper.Map(
            dbDepartment,
            userData == null ?
              null :
              _departmentUserMapper.Map(
                _userMapper.Map(userData, imagesData?.FirstOrDefault(i => i.ImageId == userData.ImageId)),
                dbDepartment.Users.FirstOrDefault(du => du.UserId == userData.Id),
                positionsData?.FirstOrDefault(p => p.Users.Select(u => u.UserId).Contains(userData.Id)))));
      }
      response.TotalCount = totalCount;

      response.Status = response.Errors.Any() ?
        OperationResultStatusType.PartialSuccess :
        OperationResultStatusType.FullSuccess;

      return response;
    }
  }
}
