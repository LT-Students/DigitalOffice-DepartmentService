using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Broker.Requests.Interfaces;
using LT.DigitalOffice.DepartmentService.Business.Department.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department.Filters;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Kernel.Validators.Interfaces;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Models;
using StackExchange.Redis;

namespace LT.DigitalOffice.DepartmentService.Business.Department
{
  public class FindDepartmentsCommand : IFindDepartmentsCommand
  {
    private readonly IDepartmentRepository _repository;
    private readonly IDepartmentInfoMapper _departmentMapper;
    private readonly IUserInfoMapper _userMapper;
    private readonly IUserService _userService;
    private readonly IBaseFindFilterValidator _baseFindValidator;
    private readonly IResponseCreator _responseCreator;

    public FindDepartmentsCommand(
      IBaseFindFilterValidator baseFindValidator,
      IDepartmentRepository repository,
      IDepartmentInfoMapper departmentMapper,
      IUserInfoMapper UserMapper,
      IUserService userService,
      IResponseCreator responseCreator)
    {
      _baseFindValidator = baseFindValidator;
      _repository = repository;
      _departmentMapper = departmentMapper;
      _userMapper = UserMapper;
      _userService = userService;
      _responseCreator = responseCreator;
    }

    public async Task<FindResultResponse<DepartmentInfo>> ExecuteAsync(FindDepartmentFilter filter, CancellationToken cancellationToken = default)
    {
      if (!_baseFindValidator.ValidateCustom(filter, out List<string> errors))
      {
        return _responseCreator.CreateFailureFindResponse<DepartmentInfo>(
          HttpStatusCode.BadRequest, errors);
      }

      (List<DbDepartment> dbDepartments, int totalCount) = await _repository.FindAsync(filter, cancellationToken);

      if (dbDepartments is null || !dbDepartments.Any())
      {
        return new FindResultResponse<DepartmentInfo>(body: new List<DepartmentInfo>(), errors: errors);
      }

      Dictionary<Guid, Guid> departmentsDirectors =
        dbDepartments
          .SelectMany(d => d.Users.Where(u => u.Assignment == (int)DepartmentUserAssignment.Director))
          .ToDictionary(d => d.DepartmentId, d => d.UserId);

      List<UserData> usersData = await _userService.GetUsersDatasAsync(
        departmentsDirectors.Values.ToList(),
        errors,
        cancellationToken);

      UserData userData = null;

      return new FindResultResponse<DepartmentInfo>(
        totalCount: totalCount,
        body: dbDepartments.Select(d =>
        {
          userData = departmentsDirectors.ContainsKey(d.Id)
            ? usersData.FirstOrDefault(u => u.Id == departmentsDirectors[d.Id])
            : null;

          return _departmentMapper.Map(
            d,
            _userMapper.Map(
              dbDepartmentUser: d.Users?.FirstOrDefault(u => u.Assignment == (int)DepartmentUserAssignment.Director),
              userData: userData,
              userPosition: null));
        }).ToList());
    }
  }
}
