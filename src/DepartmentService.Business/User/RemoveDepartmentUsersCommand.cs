using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.User.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.DepartmentService.Business.User
{
  public class RemoveDepartmentUsersCommand : IRemoveDepartmentUsersCommand
  {
    private readonly IDepartmentUserRepository _repository;
    private readonly IAccessValidator _accessValidator;
    private readonly IResponseCreator _responseCreater;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RemoveDepartmentUsersCommand(
      IDepartmentUserRepository repository,
      IAccessValidator accessValidator,
      IResponseCreator responseCreater,
      IHttpContextAccessor httpContextAccessor)
    {
      _repository = repository;
      _accessValidator = accessValidator;
      _responseCreater = responseCreater;
      _httpContextAccessor = httpContextAccessor;
    }
    public async Task<OperationResultResponse<bool>> ExecuteAsync(Guid departmentId, List<Guid> usersIds)
    {
      DbDepartmentUser sender = await _repository.GetAsync(_httpContextAccessor.HttpContext.GetUserId());

      if (!await _accessValidator.HasRightsAsync(Rights.AddEditRemoveDepartments) &&
        !(await _accessValidator.HasRightsAsync(Rights.EditDepartmentUsers) && sender != null && sender.DepartmentId == departmentId))
      {
        return _responseCreater.CreateFailureResponse<bool>(HttpStatusCode.Forbidden);
      }

      if (usersIds == null || !usersIds.Any())
      {
        return _responseCreater.CreateFailureResponse<bool>(HttpStatusCode.BadRequest);
      }

      OperationResultResponse<bool> response = new();

      response.Body = await _repository.RemoveAsync(departmentId, usersIds);
      response.Status = OperationResultStatusType.FullSuccess;

      if (!response.Body)
      {
        response = _responseCreater.CreateFailureResponse<bool>(HttpStatusCode.NotFound);
      }

      return response;
    }
  }
}
