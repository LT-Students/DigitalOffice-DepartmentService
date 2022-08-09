using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation.Results;
using LT.DigitalOffice.DepartmentService.Business.Department.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Constants;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department;
using LT.DigitalOffice.DepartmentService.Validation.Department.Interfaces;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace LT.DigitalOffice.DepartmentService.Business.Department
{
  public class CreateDepartmentCommand : ICreateDepartmentCommand
  {
    private readonly ICreateDepartmentRequestValidator _validator;
    private readonly IDbDepartmentMapper _departmentMapper;
    private readonly IDbDepartmentUserMapper _userMapper;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IDepartmentUserRepository _userRepository;
    private readonly IAccessValidator _accessValidator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IResponseCreator _responseCreator;
    private readonly IMemoryCache _cache;

    public CreateDepartmentCommand(
      ICreateDepartmentRequestValidator validator,
      IDbDepartmentMapper departmentMapper,
      IDbDepartmentUserMapper userMapper,
      IDepartmentRepository departmentRepository,
      IDepartmentUserRepository userRepository,
      IAccessValidator accessValidator,
      IHttpContextAccessor httpContextAccessor,
      IResponseCreator responseCreator,
      IMemoryCache cache)
    {
      _validator = validator;
      _departmentMapper = departmentMapper;
      _userMapper = userMapper;
      _departmentRepository = departmentRepository;
      _userRepository = userRepository;
      _accessValidator = accessValidator;
      _httpContextAccessor = httpContextAccessor;
      _responseCreator = responseCreator;
      _cache = cache;
    }

    public async Task<OperationResultResponse<Guid?>> ExecuteAsync(CreateDepartmentRequest request)
    {
      if (!await _accessValidator.HasRightsAsync(Rights.AddEditRemoveDepartments))
      {
        return _responseCreator.CreateFailureResponse<Guid?>(HttpStatusCode.Forbidden);
      }

      ValidationResult validationResult = await _validator.ValidateAsync(request);

      if (!validationResult.IsValid)
      {
        return _responseCreator.CreateFailureResponse<Guid?>(
          HttpStatusCode.BadRequest,
          validationResult.Errors.Select(vf => vf.ErrorMessage).ToList());
      }

      OperationResultResponse<Guid?> response = new();

      response.Body = await _departmentRepository.CreateAsync(_departmentMapper.Map(request));

      if (response.Body is null)
      {
        response = _responseCreator.CreateFailureResponse<Guid?>(HttpStatusCode.BadRequest);
        return response;
      }

      _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;

      List<DbDepartmentUser> dbDepartmentUsers = request.Users.Select(u => _userMapper.Map(u, response.Body.Value)).ToList();

      List<Guid> updatedUsersIds = await _userRepository.EditAsync(dbDepartmentUsers);
      await _userRepository.CreateAsync(dbDepartmentUsers.Where(du => !updatedUsersIds.Contains(du.UserId)).ToList());

      _cache.Remove(CacheKeys.DepartmentsTree);

      return response;
    }
  }
}
