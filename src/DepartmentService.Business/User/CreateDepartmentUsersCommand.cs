using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation.Results;
using LT.DigitalOffice.DepartmentService.Business.User.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Validation.Interfaces;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.DepartmentService.Business.User
{
  public class CreateDepartmentUsersCommand : ICreateDepartmentUsersCommand
  {
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAccessValidator _accessValidator;
    private readonly IDepartmentUsersValidator _validator;
    private readonly IDbDepartmentUserMapper _mapper;
    private readonly IDepartmentUserRepository _repository;
    private readonly IResponseCreator _responseCreator;

    public CreateDepartmentUsersCommand(
      IHttpContextAccessor httpContextAccessor,
      IAccessValidator accessValidator,
      IDepartmentUsersValidator validator,
      IDbDepartmentUserMapper mapper,
      IDepartmentUserRepository repository,
      IResponseCreator responseCreator)
    {
      _httpContextAccessor = httpContextAccessor;
      _accessValidator = accessValidator;
      _validator = validator;
      _mapper = mapper;
      _repository = repository;
      _responseCreator = responseCreator;
    }

    public async Task<OperationResultResponse<bool>> ExecuteAsync(Guid departmentId, List<Guid> usersIds)
    {
      if (!await _accessValidator.HasRightsAsync(Rights.AddEditRemoveDepartments) &&
        !(await _accessValidator.HasRightsAsync(Rights.EditDepartmentUsers) &&
        (await _repository.GetAsync(_httpContextAccessor.HttpContext.GetUserId()))?.DepartmentId == departmentId))
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.Forbidden);
      }

      ValidationResult validationResult = await _validator.ValidateAsync(usersIds);

      if (!validationResult.IsValid)
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.BadRequest);
      }

      OperationResultResponse<bool> response = new();

      await _repository.RemoveAsync(usersIds);

      response.Body = await _repository.CreateAsync(
        usersIds.Select(userId => _mapper.Map(userId, departmentId)).ToList());

      _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;

      response.Status = OperationResultStatusType.FullSuccess;

      if (!response.Body)
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.BadRequest);
      }

      return response;
    }
  }
}
