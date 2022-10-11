using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation.Results;
using LT.DigitalOffice.DepartmentService.Broker.Helpers.Branch.Interfaces;
using LT.DigitalOffice.DepartmentService.Business.User.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.DepartmentUser;
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
  public class EditDepartmentUsersRoleCommand : IEditDepartmentUsersRoleCommand
  {
    private readonly IDepartmentUserRepository _repository;
    private readonly IEditDepartmentUsersRoleRequestValidator _validator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAccessValidator _accessValidator;
    private readonly IResponseCreator _responseCreator;
    private readonly IGlobalCacheRepository _globalCache;
    private readonly IDepartmentBranchHelper _departmentBranchHelper;

    public EditDepartmentUsersRoleCommand(
      IDepartmentUserRepository repository,
      IEditDepartmentUsersRoleRequestValidator validator,
      IHttpContextAccessor httpContextAccessor,
      IAccessValidator accessValidator,
      IResponseCreator responseCreator,
      IGlobalCacheRepository globalCache,
      IDepartmentBranchHelper departmentBranchHelper)
    {
      _repository = repository;
      _validator = validator;
      _httpContextAccessor = httpContextAccessor;
      _accessValidator = accessValidator;
      _responseCreator = responseCreator;
      _globalCache = globalCache;
      _departmentBranchHelper = departmentBranchHelper;
    }

    public async Task<OperationResultResponse<bool>> ExecuteAsync(Guid departmentId, EditDepartmentUsersRoleRequest request)
    {
      if ((await _departmentBranchHelper.GetDepartmentUserRole(
          userId: _httpContextAccessor.HttpContext.GetUserId(),
          departmentId: departmentId) != DepartmentUserRole.Manager)
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

      OperationResultResponse<bool> response = new(await _repository.EditRoleAsync(request.UsersIds, request.Role));

      if (response.Body)
      {
        await _globalCache.RemoveAsync(departmentId);
      }
      else
      {
        response = _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.BadRequest);
      }

      return response;
    }
  }
}
