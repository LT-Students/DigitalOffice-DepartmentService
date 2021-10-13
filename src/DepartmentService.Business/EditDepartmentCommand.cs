using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Filters;
using LT.DigitalOffice.DepartmentService.Validation.Interfaces;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace LT.DigitalOffice.DepartmentService.Business
{
  public class EditDepartmentCommand : IEditDepartmentCommand
  {
    private readonly IEditDepartmentRequestValidator _validator;
    private readonly IDepartmentRepository _repository;
    private readonly IDepartmentUserRepository _userRepository;
    private readonly IPatchDbDepartmentMapper _mapper;
    private readonly IAccessValidator _accessValidator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EditDepartmentCommand(
      IEditDepartmentRequestValidator validator,
      IDepartmentRepository repository,
      IDepartmentUserRepository userRepository,
      IPatchDbDepartmentMapper mapper,
      IAccessValidator accessValidator,
      IHttpContextAccessor httpContextAccessor)
    {
      _validator = validator;
      _repository = repository;
      _userRepository = userRepository;
      _mapper = mapper;
      _accessValidator = accessValidator;
      _httpContextAccessor = httpContextAccessor;
    }

    public async Task<OperationResultResponse<bool>> ExecuteAsync(Guid departmentId, JsonPatchDocument<EditDepartmentRequest> request)
    {
      if (!(await _accessValidator.HasRightsAsync(Rights.AddEditRemoveDepartments)))
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;

        return new OperationResultResponse<bool>
        {
          Status = OperationResultStatusType.Failed,
          Errors = new() { "Not enough rights." }
        };
      }

      if (!_validator.ValidateCustom(request, out List<string> errors))
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        return new OperationResultResponse<bool>
        {
          Status = OperationResultStatusType.Failed,
          Errors = errors
        };
      }

      DbDepartment dbDepartment = await _repository.GetAsync(new GetDepartmentFilter { DepartmentId = departmentId });

      if (dbDepartment == null)
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;

        return new OperationResultResponse<bool>
        {
          Status = OperationResultStatusType.Failed,
          Errors = new() { $"Department with id: '{departmentId}' doesn't exist." }
        };
      }

      OperationResultResponse<bool> response = new();

      foreach (Operation<EditDepartmentRequest> item in request.Operations)
      {
        if (item.path.EndsWith(nameof(EditDepartmentRequest.Name), StringComparison.OrdinalIgnoreCase) &&
            await _repository.DoesNameExistAsync(item.value.ToString()))
        {
          _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;

          response.Status = OperationResultStatusType.Failed;
          response.Errors.Add("The department name already exists");
          return response;
        }
      }

      Operation<EditDepartmentRequest> directorOperation = request.Operations
        .FirstOrDefault(o => o.path.EndsWith(nameof(EditDepartmentRequest.DirectorId), StringComparison.OrdinalIgnoreCase));

      if (directorOperation != null &&
        !(await _userRepository.ChangeDirectorAsync(departmentId, Guid.Parse(directorOperation.value?.ToString()))))
      {
        response.Errors.Add("Cannot change department director.");
      }

      response.Body = await _repository.EditAsync(dbDepartment, _mapper.Map(request));
      response.Status = response.Body ? OperationResultStatusType.FullSuccess : OperationResultStatusType.Failed;
      return response;
    }
  }
}
