using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.User.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.DepartmentService.Business.User
{
  public class RemoveDepartmentUserRequest : IRemoveDepartmentUserRequest
  {
    private readonly IDepartmentUserRepository _repository;
    private readonly IAccessValidator _accessValidator;
    private readonly IResponseCreater _responseCreater;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RemoveDepartmentUserRequest(
      IDepartmentUserRepository repository,
      IAccessValidator accessValidator,
      IResponseCreater responseCreater,
      IHttpContextAccessor httpContextAccessor)
    {
      _repository = repository;
      _accessValidator = accessValidator;
      _responseCreater = responseCreater;
      _httpContextAccessor = httpContextAccessor;
    }
    public async Task<OperationResultResponse<bool>> ExecuteAsync(Guid departmentId, List<Guid> usersIds)
    {
      if (!await _accessValidator.HasRightsAsync(Rights.AddEditRemoveProjects)
        && !await _accessValidator.HasRightsAsync(Rights.EditDepartmentUsers))
      {
        return _responseCreater.CreateFailureResponse<bool>(HttpStatusCode.Forbidden);
      }

      if (usersIds == null || !usersIds.Any())
      {
        return _responseCreater.CreateFailureResponse<bool>(HttpStatusCode.BadRequest);
      }

      bool response = await _repository.RemoveAsync(departmentId, usersIds);

      return new()
      {
        Status = response ? OperationResultStatusType.FullSuccess : OperationResultStatusType.Failed,
        Body = response
      };
    }
  }
}
