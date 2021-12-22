using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.User.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.DepartmentService.Business.User
{
  public class RemoveDepartmentUsersCommand : IRemoveDepartmentUsersCommand
  {
    private readonly IDepartmentUserRepository _repository;
    private readonly IAccessValidator _accessValidator;
    private readonly IResponseCreator _responseCreator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICacheNotebook _cacheNotebook;

    public RemoveDepartmentUsersCommand(
      IDepartmentUserRepository repository,
      IAccessValidator accessValidator,
      IResponseCreator responseCreator,
      IHttpContextAccessor httpContextAccessor,
      ICacheNotebook cacheNotebook)
    {
      _repository = repository;
      _accessValidator = accessValidator;
      _responseCreator = responseCreator;
      _httpContextAccessor = httpContextAccessor;
      _cacheNotebook = cacheNotebook;
    }
    public async Task<OperationResultResponse<bool>> ExecuteAsync(Guid departmentId, List<Guid> usersIds)
    {
      if (!await _accessValidator.HasRightsAsync(Rights.AddEditRemoveDepartments) &&
        !(await _accessValidator.HasRightsAsync(Rights.EditDepartmentUsers) &&
        (await _repository.GetAsync(_httpContextAccessor.HttpContext.GetUserId()))?.DepartmentId == departmentId))
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.Forbidden);
      }

      if (usersIds == null || !usersIds.Any())
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.BadRequest);
      }

      OperationResultResponse<bool> response = new();

      response.Body = await _repository.RemoveAsync(departmentId, usersIds);

      if (!response.Body)
      {
        response = _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.NotFound);
      }
      else
      {
        usersIds.Select(async i => await _cacheNotebook.RemoveAsync(i));
      }

      return response;
    }
  }
}
