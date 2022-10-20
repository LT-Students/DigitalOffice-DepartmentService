using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Broker.Helpers.Branch.Interfaces;
using LT.DigitalOffice.DepartmentService.Business.User.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
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
  public class RemoveDepartmentUsersCommand : IRemoveDepartmentUsersCommand
  {
    private readonly IDepartmentUserRepository _repository;
    private readonly IAccessValidator _accessValidator;
    private readonly IResponseCreator _responseCreator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IGlobalCacheRepository _globalChache;
    private readonly IDepartmentBranchHelper _departmentBranchHelper;

    public RemoveDepartmentUsersCommand(
      IDepartmentUserRepository repository,
      IAccessValidator accessValidator,
      IResponseCreator responseCreator,
      IHttpContextAccessor httpContextAccessor,
      IGlobalCacheRepository globalCache,
      IDepartmentBranchHelper departmentBranchHelper)
    {
      _repository = repository;
      _accessValidator = accessValidator;
      _responseCreator = responseCreator;
      _httpContextAccessor = httpContextAccessor;
      _globalChache = globalCache;
      _departmentBranchHelper = departmentBranchHelper;
    }
    public async Task<OperationResultResponse<bool>> ExecuteAsync(Guid departmentId, List<Guid> usersIds)
    {
      if ((await _departmentBranchHelper.GetDepartmentUserRole(
          userId: _httpContextAccessor.HttpContext.GetUserId(),
          departmentId: departmentId) != DepartmentUserRole.Manager)
        && !await _accessValidator.HasRightsAsync(Rights.AddEditRemoveDepartments))
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.Forbidden);
      }

      if (usersIds is null || !usersIds.Any())
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.BadRequest);
      }

      await _repository.RemoveAsync(departmentId, usersIds);

      await _globalChache.RemoveAsync(departmentId);

      return new()
      {
        Body = true
      };
    }
  }
}
