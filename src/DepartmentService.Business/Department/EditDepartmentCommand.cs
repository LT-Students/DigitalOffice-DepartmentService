using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation.Results;
using LT.DigitalOffice.DepartmentService.Business.Department.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Patch.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department;
using LT.DigitalOffice.DepartmentService.Validation.Department.Interfaces;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.DepartmentService.Business.Department
{
  public class EditDepartmentCommand : IEditDepartmentCommand
  {
    private readonly IEditDepartmentRequestValidator _validator;
    private readonly IDepartmentRepository _repository;
    private readonly IDepartmentUserRepository _userRepository;
    private readonly IPatchDbDepartmentMapper _mapper;
    private readonly IAccessValidator _accessValidator;
    private readonly IResponseCreator _responseCreator;
    private readonly IGlobalCacheRepository _globalCache;

    public EditDepartmentCommand(
      IEditDepartmentRequestValidator validator,
      IDepartmentRepository repository,
      IDepartmentUserRepository userRepository,
      IPatchDbDepartmentMapper mapper,
      IAccessValidator accessValidator,
      IResponseCreator responseCreator,
      IGlobalCacheRepository globalCache)
    {
      _validator = validator;
      _repository = repository;
      _userRepository = userRepository;
      _mapper = mapper;
      _accessValidator = accessValidator;
      _responseCreator = responseCreator;
      _globalCache = globalCache;
    }

    public async Task<OperationResultResponse<bool>> ExecuteAsync(Guid departmentId, JsonPatchDocument<EditDepartmentRequest> request)
    {
      if (!await _accessValidator.HasRightsAsync(Rights.AddEditRemoveDepartments))
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.Forbidden);
      }

      ValidationResult validationResult = await _validator.ValidateAsync(request);

      if (!validationResult.IsValid)
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.BadRequest,
          validationResult.Errors.Select(vf => vf.ErrorMessage).ToList());
      }

      OperationResultResponse<bool> response = new();

      response.Body = await _repository.EditAsync(departmentId, _mapper.Map(request));

      object isActiveOperation = request.Operations.FirstOrDefault(o =>
        o.path.EndsWith(nameof(EditDepartmentRequest.IsActive), StringComparison.OrdinalIgnoreCase))?.value;

      if (isActiveOperation != null && bool.Parse(isActiveOperation.ToString()))
      {
        await _userRepository.RemoveAsync(departmentId);
      }

      await _globalCache.RemoveAsync(departmentId);

      return response;
    }
  }
}
