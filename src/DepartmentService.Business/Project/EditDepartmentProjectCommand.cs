using System;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.Project.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.UserService.Business.Commands.User
{
  public class EditDepartmentProjectCommand : IEditDepartmentProjectCommand
  {
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAccessValidator _accessValidator;
    private readonly IDbDepartmentProjectMapper _mapper;
    private readonly IDepartmentProjectRepository _projectRepository;
    private readonly IDepartmentUserRepository _userRepository;
    private readonly IResponseCreator _responseCreator;

    public EditDepartmentProjectCommand(
      IHttpContextAccessor httpContextAccessor,
      IAccessValidator accessValidator,
      IDbDepartmentProjectMapper mapper,
      IDepartmentProjectRepository projectRepository,
      IDepartmentUserRepository userRepository,
      IResponseCreator responseCreator)
    {
      _httpContextAccessor = httpContextAccessor;
      _accessValidator = accessValidator;
      _mapper = mapper;
      _projectRepository = projectRepository;
      _userRepository = userRepository;
      _responseCreator = responseCreator;
    }

    public async Task<OperationResultResponse<bool>> ExecuteAsync(EditDepartmentProjectRequest request)
    {
      if (!await _userRepository.IsManagerAsync(_httpContextAccessor.HttpContext.GetUserId())
        && !await _accessValidator.HasRightsAsync(Rights.AddEditRemoveDepartments))
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.Forbidden);
      }

      if (!await _projectRepository.EditAsync(request.ProjectId, request.DepartmentId))
      {
        await _projectRepository.CreateAsync(
          _mapper.Map(request.ProjectId, request.DepartmentId.Value, _httpContextAccessor.HttpContext.GetUserId()));
      }

      _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;

      return new()
      {
        Body = true
      };
    }
  }
}
