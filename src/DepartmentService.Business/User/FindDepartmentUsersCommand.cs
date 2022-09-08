using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Broker.Requests.Interfaces;
using LT.DigitalOffice.DepartmentService.Business.User.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.DepartmentUser;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Kernel.Validators.Interfaces;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Position;
using Pipelines.Sockets.Unofficial.Arenas;

namespace LT.DigitalOffice.DepartmentService.Business.User
{
  public class FindDepartmentUsersCommand : IFindDepartmentUsersCommand
  {
    private readonly IBaseFindFilterValidator _baseFindFilterValidator;
    private readonly IDepartmentUserRepository _departmentUserRepository;
    private readonly IUserService _userService;
    private readonly IImageService _imageService;
    private readonly IPositionService _positionService;
    private readonly IUserInfoMapper _userInfoMapper;

    public FindDepartmentUsersCommand(
      IBaseFindFilterValidator baseFindFilterValidator,
      IDepartmentUserRepository projectUserRepository,
      IUserService userService,
      IImageService imageService,
      IPositionService positionService,
      IUserInfoMapper userInfoMapper)
    {
      _baseFindFilterValidator = baseFindFilterValidator;
      _departmentUserRepository = projectUserRepository;
      _userService = userService;
      _imageService = imageService;
      _positionService = positionService;
      _userInfoMapper = userInfoMapper;
    }

    public async Task<FindResultResponse<UserInfo>> ExecuteAsync(
      Guid departmentId,
      FindDepartmentUsersFilter filter,
      CancellationToken cancellationToken = default)
    {
      if (!_baseFindFilterValidator.ValidateCustom(filter, out List<string> errors))
      {
        return ResponseCreatorStatic.CreateFindResponse<UserInfo>(statusCode: HttpStatusCode.BadRequest, errors: errors);
      }

      List<DbDepartmentUser> departmentUsers = 
        await _departmentUserRepository.GetAsync(departmentId: departmentId, filter: filter, cancellationToken);

      if (departmentUsers is null || !departmentUsers.Any())
      {
        return new(errors: errors);
      }

      IEnumerable<Guid> usersIds = departmentUsers.Select(pu => pu.UserId);

      //should fix it in future
      //filter department users by posinion
      if (filter.byPositionId.HasValue)
      {
        PositionFilteredData data = (await _positionService.GetPositionFilteredDataAsync(new List<Guid>() { filter.byPositionId.Value }, errors))?.FirstOrDefault();

        usersIds = data is not null ? usersIds.Where(i => data.UsersIds.Contains(i)).ToList() : Enumerable.Empty<Guid>();
      }

      (List<UserData> usersData, int totalCount) = await _userService.GetFilteredUsersAsync(usersIds.ToList(), filter, cancellationToken);

      Task<List<ImageInfo>> usersAvatarsTask = filter.IncludeAvatars
        ? _imageService.GetImagesAsync(
            imagesIds: usersData?.Where(x => x.ImageId.HasValue).Select(x => x.ImageId.Value).ToList(),
            imageSourse: ImageSource.User,
            errors,
            cancellationToken)
        : Task.FromResult<List<ImageInfo>>(default);

      Task<List<PositionData>> usersPositionsTask = filter.IncludePositions
        ? _positionService.GetPositionsAsync(usersIds: usersData?.Select(x => x.Id).ToList(), errors, cancellationToken)
        : Task.FromResult<List<PositionData>>(default);

      if (filter.DepartmentUserRoleAscendingSort.HasValue)
      {
        usersData = departmentUsers
          .Select(du => usersData.FirstOrDefault(u => u.Id == du.UserId))
          .Where(u => u is not null).ToList();
      }

      List<ImageInfo> usersAvatars = await usersAvatarsTask;
      List<PositionData> usersPositions = await usersPositionsTask;

      return new FindResultResponse<UserInfo>(
        errors: errors,
        totalCount: totalCount,
        body: usersData?
          .Select(userData => _userInfoMapper.Map(
            dbDepartmentUser: departmentUsers.FirstOrDefault(pu => pu.UserId == userData.Id),
            userData: userData,
            image: usersAvatars?.FirstOrDefault(av => av.Id == userData.ImageId),
            userPosition: usersPositions?.FirstOrDefault(p => p.UsersIds.Contains(userData.Id))))
          .ToList());
    }
  }
}
