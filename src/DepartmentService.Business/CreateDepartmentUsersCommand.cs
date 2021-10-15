using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation.Results;
using LT.DigitalOffice.DepartmentService.Business.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.DepartmentService.Validation.Interfaces;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.DepartmentService.Business
{
  public class CreateDepartmentUsersCommand : ICreateDepartmentUsersCommand
  {
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAccessValidator _accessValidator;
    private readonly ICreateDepartmentUsersRequestValidator _validator;
    private readonly IDbDepartmentUserMapper _mapper;
    private readonly IDepartmentUserRepository _repository;

    public CreateDepartmentUsersCommand(
      IHttpContextAccessor httpContextAccessor,
      IAccessValidator accessValidator,
      ICreateDepartmentUsersRequestValidator validator,
      IDbDepartmentUserMapper mapper,
      IDepartmentUserRepository repository)
    {
      _httpContextAccessor = httpContextAccessor;
      _accessValidator = accessValidator;
      _validator = validator;
      _mapper = mapper;
      _repository = repository;
    }

    public async Task<OperationResultResponse<bool>> ExecuteAsync(CreateDepartmentUsersRequest request)
    {
      Guid senderUserId = _httpContextAccessor.HttpContext.GetUserId();

      if (!(await _accessValidator.HasRightsAsync(Rights.AddEditRemoveDepartments) &&
        await _repository.IsDepartmentDirectorAsync(request.DepartmentId, senderUserId)))
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;

        return new OperationResultResponse<bool>
        {
          Status = OperationResultStatusType.Failed
        };
      }

      ValidationResult validationResult = await _validator.ValidateAsync(request);

      if (!validationResult.IsValid)
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        return new OperationResultResponse<bool>
        {
          Status = OperationResultStatusType.Failed,
          Errors = validationResult.Errors.Select(vf => vf.ErrorMessage).ToList()
        };
      }

      OperationResultResponse<bool> response = new();

      _repository.RemoveAsync(request.UsersIds, senderUserId);

      response.Body = await _repository.AddAsync(
        request.UsersIds.Select(x => _mapper.Map(x, request.DepartmentId, senderUserId)).ToList());

      _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;

      response.Status = OperationResultStatusType.FullSuccess;

      if (!response.Body)
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        response.Status = OperationResultStatusType.Failed;
      }

      return response;
    }
  }
}
