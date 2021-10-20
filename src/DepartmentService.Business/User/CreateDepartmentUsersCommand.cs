using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation.Results;
using LT.DigitalOffice.DepartmentService.Business.User.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.DepartmentService.Validation.Interfaces;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.DepartmentService.Business.User
{
  public class CreateDepartmentUsersCommand : ICreateDepartmentUsersCommand
  {
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAccessValidator _accessValidator;
    private readonly ICreateDepartmentUsersRequestValidator _validator;
    private readonly IDbDepartmentUserMapper _mapper;
    private readonly IDepartmentUserRepository _repository;
    private readonly IResponseCreater _responseCreater;

    public CreateDepartmentUsersCommand(
      IHttpContextAccessor httpContextAccessor,
      IAccessValidator accessValidator,
      ICreateDepartmentUsersRequestValidator validator,
      IDbDepartmentUserMapper mapper,
      IDepartmentUserRepository repository,
      IResponseCreater responseCreater)
    {
      _httpContextAccessor = httpContextAccessor;
      _accessValidator = accessValidator;
      _validator = validator;
      _mapper = mapper;
      _repository = repository;
      _responseCreater = responseCreater;
    }

    public async Task<OperationResultResponse<bool>> ExecuteAsync(CreateDepartmentUsersRequest request)
    {
      if (!await _accessValidator.HasRightsAsync(Rights.EditDepartmentUsers) &&
        !await _accessValidator.HasRightsAsync(Rights.AddEditRemoveDepartments))
      {
        return _responseCreater.CreateFailureResponse<bool>(HttpStatusCode.Forbidden);
      }

      ValidationResult validationResult = await _validator.ValidateAsync(request);

      if (!validationResult.IsValid)
      {
        return _responseCreater.CreateFailureResponse<bool>(HttpStatusCode.BadRequest);
      }

      OperationResultResponse<bool> response = new();

      await _repository.RemoveAsync(request.Users);

      response.Body = await _repository.CreateAsync(
        request.Users.Select(x => _mapper.Map(x, request.DepartmentId)).ToList());

      _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;

      response.Status = OperationResultStatusType.FullSuccess;

      if (!response.Body)
      {
        return _responseCreater.CreateFailureResponse<bool>(HttpStatusCode.BadRequest);
      }

      return response;
    }
  }
}
