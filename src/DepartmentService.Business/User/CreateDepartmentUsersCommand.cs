using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation.Results;
using LT.DigitalOffice.DepartmentService.Broker.Helpers.Branch.Interfaces;
using LT.DigitalOffice.DepartmentService.Business.User.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.DepartmentService.Validation.DepartmentUser.Interfaces;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Enums;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.DepartmentService.Business.User
{
  public class CreateDepartmentUsersCommand : ICreateDepartmentUsersCommand
  {
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAccessValidator _accessValidator;
    private readonly ICreateDepartmentUsersValidator _validator;
    private readonly IDbDepartmentUserMapper _mapper;
    private readonly IDepartmentUserRepository _repository;
    private readonly IDepartmentBranchHelper _departmentBranchHelper;
    private readonly IResponseCreator _responseCreator;
    private readonly IGlobalCacheRepository _globalCache;

    public CreateDepartmentUsersCommand(
      IHttpContextAccessor httpContextAccessor,
      IAccessValidator accessValidator,
      ICreateDepartmentUsersValidator validator,
      IDbDepartmentUserMapper mapper,
      IDepartmentUserRepository repository,
      IDepartmentBranchHelper departmentBranchHelper,
      IResponseCreator responseCreator,
      IGlobalCacheRepository globalCache)
    {
      _httpContextAccessor = httpContextAccessor;
      _accessValidator = accessValidator;
      _validator = validator;
      _mapper = mapper;
      _repository = repository;
      _departmentBranchHelper = departmentBranchHelper;
      _responseCreator = responseCreator;
      _globalCache = globalCache;
    }

    public async Task<OperationResultResponse<bool>> ExecuteAsync(CreateDepartmentUsersRequest request)
    {
      if ((await _departmentBranchHelper.GetDepartmentUserRole(
          userId: _httpContextAccessor.HttpContext.GetUserId(),
          departmentId: request.DepartmentId) != DepartmentUserRole.Manager)
        && !await _accessValidator.HasRightsAsync(Rights.AddEditRemoveDepartments))
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.Forbidden);
      }

      ValidationResult validationResult = await _validator.ValidateAsync(request);

      if (!validationResult.IsValid)
      {
        return _responseCreator.CreateFailureResponse<bool>(
          HttpStatusCode.BadRequest,
          validationResult.Errors.Select(ValidationFailure => ValidationFailure.ErrorMessage).ToList());
      }
      
      if (request.Users.Where(u => u.Assignment == DepartmentUserAssignment.Director).Any())
      {
        await _repository.RemoveDirectorAsync(request.DepartmentId);
      }

      List<DbDepartmentUser> dbDepartmentUsers = request.Users
        .Select(u => _mapper.Map(u, request.DepartmentId)).ToList();

      List<Guid> updatedUsersIds = await _repository.EditAsync(dbDepartmentUsers);

      await _repository.CreateAsync(dbDepartmentUsers.Where(du => !updatedUsersIds.Contains(du.UserId)).ToList());

      _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;

      updatedUsersIds?.ForEach(async userId => await _globalCache.RemoveAsync(userId));
      await _globalCache.RemoveAsync(request.DepartmentId);

      return new OperationResultResponse<bool>(body: true);
    }
  }
}
