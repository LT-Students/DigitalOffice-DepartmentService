using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Broker.Requests.Interfaces;
using LT.DigitalOffice.DepartmentService.Business.Department.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Responses.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department.Filters;
using LT.DigitalOffice.DepartmentService.Models.Dto.Responses;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Project;
using StackExchange.Redis;

namespace LT.DigitalOffice.DepartmentService.Business.Department
{
  public class GetDepartmentCommand : IGetDepartmentCommand
  {
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IDepartmentResponseMapper _departmentResponseMapper;
    private readonly IUserInfoMapper _UserInfoMapper;
    private readonly IDepartmentUserInfoMapper _departmentUserInfoMapper;
    private readonly IProjectInfoMapper _projectInfoMapper;
    private readonly IUserService _userService;
    private readonly IImageService _imageService;
    private readonly IProjectService _projectService;
    private readonly IConnectionMultiplexer _cache;
    private readonly IResponseCreator _responseCreator;

    public GetDepartmentCommand(
      IDepartmentRepository departmentRepository,
      IDepartmentResponseMapper departmentResponseMapper,
      IUserInfoMapper UserInfoMapper,
      IDepartmentUserInfoMapper departmmentUserInfoMapper,
      IProjectInfoMapper projectInfoMapper,
      IUserService userService,
      IImageService imageService,
      IProjectService projectService,
      IConnectionMultiplexer cache,
      IResponseCreator responseCreator)
    {
      _cache = cache;
      _userService = userService;
      _imageService = imageService;
      _projectService = projectService;
      _departmentRepository = departmentRepository;
      _departmentResponseMapper = departmentResponseMapper;
      _projectInfoMapper = projectInfoMapper;
      _UserInfoMapper = UserInfoMapper;
      _departmentUserInfoMapper = departmmentUserInfoMapper;
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

      List<ProjectData> projectData = await _projectService
        .GetProjectsAsync(dbDepartment.Projects.Select(dp => dp.ProjectId).ToList(), response.Errors);

      List<Guid> usersIds = new();

      if (dbDepartment.Users is not null && dbDepartment.Users.Any())
      {
        usersIds.AddRange(dbDepartment.Users.Select(x => x.UserId).ToList());
      }

      List<UserData> usersData = await _userService.GetUsersDatasAsync(usersIds.Distinct().ToList(), response.Errors);
      List<UserInfo> usersInfo = null;
      List<DepartmentUserInfo> departmentUsersInfo = null;

      if (usersData != null && usersData.Any())
      {
        List<ImageInfo> imagesData = await _imageService.GetImagesAsync(
          usersData.Where(u => u.ImageId.HasValue).Select(u => u.ImageId.Value).ToList(),
          ImageSource.User,
          response.Errors);

        usersInfo = usersData
          .Select(u => _UserInfoMapper.Map(u, imagesData?.FirstOrDefault(i => i.Id == u.ImageId)))
          .ToList();

        departmentUsersInfo = dbDepartment.Users?.Select(
          du =>
            _departmentUserInfoMapper.Map(
              usersInfo.FirstOrDefault(u => u.Id == du.UserId),
              du)
          ).ToList();
      }

      IEnumerable<ProjectInfo> projectInfo = projectData?.Select(_projectInfoMapper.Map);

      response.Body = _departmentResponseMapper.Map(dbDepartment, departmentUsersInfo, projectInfo);

      return response;
    }
  }
}
