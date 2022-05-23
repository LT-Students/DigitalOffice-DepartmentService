using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Broker.Requests.Interfaces;
using LT.DigitalOffice.DepartmentService.Business.Department.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department.Filters;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Kernel.Validators.Interfaces;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Position;
using LT.DigitalOffice.Models.Broker.Requests.Position;
using LT.DigitalOffice.Models.Broker.Responses.Position;
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
    private readonly IUserService _userService;
    private readonly IImageService _imageService;
    private readonly IBaseFindFilterValidator _baseFindValidator;
    private readonly IResponseCreator _responseCreator;

    public FindDepartmentsCommand(
      IBaseFindFilterValidator baseFindValidator,
      IDepartmentRepository repository,
      IDepartmentInfoMapper departmentMapper,
      IUserInfoMapper UserMapper,
      IDepartmentUserInfoMapper departmentUserMapper,
      IUserService userService,
      IImageService imageService,
      IResponseCreator responseCreator)
    {
      _baseFindValidator = baseFindValidator;
      _repository = repository;
      _departmentMapper = departmentMapper;
      _userMapper = UserMapper;
      _departmentUserMapper = departmentUserMapper;
      _userService = userService;
      _imageService = imageService;
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

      if (dbDepartments is null || !dbDepartments.Any())
      {
        return response;
      }

      Dictionary<Guid, Guid> departmentsDirectors =
        dbDepartments
          .SelectMany(d => d.Users.Where(u => u.Role == (int)DepartmentUserRole.Manager))
          .ToDictionary(d => d.DepartmentId, d => d.UserId);

      List<UserData> usersData = await _userService.GetUsersDatasAsync(
        departmentsDirectors.Values.ToList(),
        response.Errors);

      List<ImageInfo> images = await _imageService.GetImagesAsync(
        usersData?.Where(ud => ud.ImageId.HasValue)?.Select(ud => ud.ImageId.Value).ToList(),
        ImageSource.User,
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
                _userMapper.Map(userData, images?.FirstOrDefault(i => i.Id == userData.ImageId)),
                dbDepartment.Users.FirstOrDefault(du => du.UserId == userData.Id))));
      }
      response.TotalCount = totalCount;

      response.Status = response.Errors.Any() ?
        OperationResultStatusType.PartialSuccess :
        OperationResultStatusType.FullSuccess;

      return response;
    }
  }
}
