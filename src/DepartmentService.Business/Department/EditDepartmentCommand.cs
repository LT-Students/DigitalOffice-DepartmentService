using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.Department.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.DepartmentService.Validation.Interfaces;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace LT.DigitalOffice.DepartmentService.Business.Department
{
  public class EditDepartmentCommand : IEditDepartmentCommand
  {
    private readonly IEditDepartmentRequestValidator _validator;
    private readonly IDepartmentRepository _repository;
    private readonly IDepartmentUserRepository _userRepository;
    private readonly IPatchDbDepartmentMapper _mapper;
    private readonly IAccessValidator _accessValidator;
    private readonly IResponseCreater _responseCreater;

    public EditDepartmentCommand(
      IEditDepartmentRequestValidator validator,
      IDepartmentRepository repository,
      IDepartmentUserRepository userRepository,
      IPatchDbDepartmentMapper mapper,
      IAccessValidator accessValidator,
      IResponseCreater responseCreater)
    {
      _validator = validator;
      _repository = repository;
      _userRepository = userRepository;
      _mapper = mapper;
      _accessValidator = accessValidator;
      _responseCreater = responseCreater;
    }

    public async Task<OperationResultResponse<bool>> ExecuteAsync(Guid departmentId, JsonPatchDocument<EditDepartmentRequest> patch)
    {
      if (!(await _accessValidator.HasRightsAsync(Rights.AddEditRemoveDepartments)))
      {
        return _responseCreater.CreateFailureResponse<bool>(HttpStatusCode.Forbidden);
      }

      if (!_validator.ValidateCustom(patch, out List<string> errors))
      {
        return _responseCreater.CreateFailureResponse<bool>(HttpStatusCode.BadRequest);
      }

      OperationResultResponse<bool> response = new();

      foreach (Operation<EditDepartmentRequest> item in patch.Operations)
      {
        if (item.path.EndsWith(nameof(EditDepartmentRequest.Name), StringComparison.OrdinalIgnoreCase) &&
            await _repository.DoesNameExistAsync(item.value.ToString()))
        {
          return _responseCreater.CreateFailureResponse<bool>(HttpStatusCode.Conflict);
        }

        Operation<EditDepartmentRequest> directorOperation = patch.Operations
          .FirstOrDefault(o => o.path.EndsWith(nameof(EditDepartmentRequest.DirectorId), StringComparison.OrdinalIgnoreCase));

        if (directorOperation != null &&
          !(await _userRepository.ChangeDirectorAsync(departmentId, Guid.Parse(directorOperation.value?.ToString()))))
        {
          response.Errors.Add("Cannot change department director.");
        }
      }

      response.Body = await _repository.EditAsync(departmentId, _mapper.Map(patch));
      response.Status = response.Body ? OperationResultStatusType.FullSuccess : OperationResultStatusType.Failed;
      return response;
    }
  }
}
