using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.DepartmentService.Validation.Interfaces;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.DepartmentService.Business
{
  public class CreateDepartmentCommand : ICreateDepartmentCommand
  {
    private readonly IDepartmentRepository _repository;
    private readonly IDepartmentUserRepository _userRepository;
    private readonly ICreateDepartmentRequestValidator _validator;
    private readonly IDbDepartmentMapper _mapper;
    private readonly IAccessValidator _accessValidator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateDepartmentCommand(
      IDepartmentRepository repository,
      IDepartmentUserRepository userRepository,
      ICreateDepartmentRequestValidator validator,
      IDbDepartmentMapper mapper,
      IAccessValidator accessValidator,
      IHttpContextAccessor httpContextAccessor)
    {
      _repository = repository;
      _userRepository = userRepository;
      _validator = validator;
      _mapper = mapper;
      _accessValidator = accessValidator;
      _httpContextAccessor = httpContextAccessor;
    }

    public async Task<OperationResultResponse<Guid>> ExecuteAsync(CreateDepartmentRequest request)
    {
      if (!await _accessValidator.HasRightsAsync(Rights.AddEditRemoveDepartments))
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;

        return new OperationResultResponse<Guid>
        {
          Status = OperationResultStatusType.Failed,
          Errors = new() { "Not enough rights." }
        };
      }

      if (!_validator.ValidateCustom(request, out List<string> errors))
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        return new OperationResultResponse<Guid>
        {
          Status = OperationResultStatusType.Failed,
          Errors = errors
        };
      }

      OperationResultResponse<Guid> response = new();

      if (await _repository.DoesNameExistAsync(request.Name))
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;

        response.Status = OperationResultStatusType.Failed;
        response.Errors.Add("The department name already exists");
        return response;
      }

      #region Deactivated previous department user records

      if (request.Users != null)
      {
        await _userRepository.RemoveAsync(request.Users, _httpContextAccessor.HttpContext.GetUserId());
      }

      if (request.DirectorUserId.HasValue)
      {
        await _userRepository.RemoveAsync(request.DirectorUserId.Value, _httpContextAccessor.HttpContext.GetUserId());
      }

      #endregion

      response.Body = await _repository.CreateAsync(_mapper.Map(request));
      response.Status = OperationResultStatusType.FullSuccess;

      _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;

      return response;
    }
  }
}
