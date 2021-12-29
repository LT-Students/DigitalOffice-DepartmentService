using System;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.Project.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.UserService.Business.Commands.User
{
  public class EditProjectCommand : IEditProjectCommand
  {
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAccessValidator _accessValidator;
    private readonly IDbDepartmentProjectMapper _mapper;
    private readonly IDepartmentProjectRepository _departmentProjectRepository;
    private readonly IResponseCreator _responseCreator;

    public EditProjectCommand(
      IHttpContextAccessor httpContextAccessor,
      IAccessValidator accessValidator,
      IDbDepartmentProjectMapper mapper,
      IDepartmentProjectRepository departmentProjectRepository,
      IResponseCreator responseCreator)
    {
      _httpContextAccessor = httpContextAccessor;
      _accessValidator = accessValidator;
      _mapper = mapper;
      _departmentProjectRepository = departmentProjectRepository;
      _responseCreator = responseCreator;
    }

    public async Task<OperationResultResponse<Guid?>> ExecuteAsync(EditDepartmentProjectRequest request)
    {
      if (!await _accessValidator.HasRightsAsync(Rights.AddEditRemoveDepartments) &&
        !(await _accessValidator.HasRightsAsync(Rights.EditDepartmentUsers) &&
        (await _departmentProjectRepository.GetAsync(_httpContextAccessor.HttpContext.GetUserId()))?.DepartmentId == request.DepartmentId))
      {
        return _responseCreator.CreateFailureResponse<Guid?>(HttpStatusCode.Forbidden);
      }

      OperationResultResponse<Guid?> response = new();

      await _departmentProjectRepository.RemoveAsync(request.ProjectId);

      if (request.DepartmentId is not null)
      {
        response.Body = await _departmentProjectRepository.CreateAsync(
          _mapper.Map(
            request.ProjectId,
            request.DepartmentId.Value,
            _httpContextAccessor.HttpContext.GetUserId()));
        response.Status = OperationResultStatusType.FullSuccess;

        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;
      }

      if (response.Body == default)
      {
        response = _responseCreator.CreateFailureResponse<Guid?>(HttpStatusCode.BadRequest);
      }

      return response;
    }
  }
}
