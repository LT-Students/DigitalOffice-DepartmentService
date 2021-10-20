using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.Department.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.DepartmentService.Validation.Interfaces;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.DepartmentService.Business.Department
{
  public class CreateDepartmentCommand : ICreateDepartmentCommand
  {
    private readonly IDepartmentRepository _repository;
    private readonly IDepartmentUserRepository _userRepository;
    private readonly ICreateDepartmentRequestValidator _validator;
    private readonly IDbDepartmentMapper _mapper;
    private readonly IAccessValidator _accessValidator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IResponseCreater _responseCreater;

    public CreateDepartmentCommand(
      IDepartmentRepository repository,
      IDepartmentUserRepository userRepository,
      ICreateDepartmentRequestValidator validator,
      IDbDepartmentMapper mapper,
      IAccessValidator accessValidator,
      IHttpContextAccessor httpContextAccessor,
      IResponseCreater responseCreater)
    {
      _repository = repository;
      _userRepository = userRepository;
      _validator = validator;
      _mapper = mapper;
      _accessValidator = accessValidator;
      _httpContextAccessor = httpContextAccessor;
      _responseCreater = responseCreater;
    }

    public async Task<OperationResultResponse<Guid>> ExecuteAsync(CreateDepartmentRequest request)
    {
      if (!await _accessValidator.HasRightsAsync(Rights.AddEditRemoveDepartments))
      {
        return _responseCreater.CreateFailureResponse<Guid>(HttpStatusCode.Forbidden);
      }

      if (!_validator.ValidateCustom(request, out List<string> errors))
      {
        return _responseCreater.CreateFailureResponse<Guid>(HttpStatusCode.BadRequest, errors);
      }

      OperationResultResponse<Guid> response = new();

      #region Deactivated previous department user records

      if (request.Users != null)
      {
        await _userRepository.RemoveAsync(request.Users);
      }

      if (request.DirectorUserId.HasValue)
      {
        await _userRepository.RemoveAsync(request.Users.FirstOrDefault(u => u.UserId == request.DirectorUserId));
      }

      #endregion

      response.Body = await _repository.CreateAsync(_mapper.Map(request));
      response.Status = OperationResultStatusType.FullSuccess;

      _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;

      return response;
    }
  }
}
