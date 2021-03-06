using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Validators;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department;
using LT.DigitalOffice.DepartmentService.Validation.Department.Interfaces;
using LT.DigitalOffice.Kernel.Validators;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace LT.DigitalOffice.DepartmentService.Validation.Department
{
  public class EditDepartmentRequestValidator : BaseEditRequestValidator<EditDepartmentRequest>, IEditDepartmentRequestValidator
  {
    private readonly IDepartmentRepository _repository;

    private async Task HandleInternalPropertyValidation(
      Operation<EditDepartmentRequest> requestedOperation,
      CustomContext context)
    {
      RequestedOperation = requestedOperation;
      Context = context;

      #region Paths

      AddСorrectPaths(
        new List<string>
        {
          nameof(EditDepartmentRequest.Name),
          nameof(EditDepartmentRequest.ShortName),
          nameof(EditDepartmentRequest.Description),
          nameof(EditDepartmentRequest.IsActive)
        });

      AddСorrectOperations(nameof(EditDepartmentRequest.Name), new() { OperationType.Replace });
      AddСorrectOperations(nameof(EditDepartmentRequest.ShortName), new() { OperationType.Replace });
      AddСorrectOperations(nameof(EditDepartmentRequest.Description), new() { OperationType.Replace });
      AddСorrectOperations(nameof(EditDepartmentRequest.IsActive), new() { OperationType.Replace });

      #endregion

      #region Name

      AddFailureForPropertyIf(
        nameof(EditDepartmentRequest.Name),
        x => x == OperationType.Replace,
        new()
        {
          { x => !string.IsNullOrEmpty(x.value?.ToString().Trim()), "Name must not be empty." },
          { x => x.value.ToString().Trim().Length > 2, "Name is too short." },
          { x => x.value.ToString().Length < 300, "Name is too long." },
        }, CascadeMode.Stop);

      await AddFailureForPropertyIfAsync(
        nameof(EditDepartmentRequest.Name),
        x => x == OperationType.Replace,
        new()
        {
          {
            async x =>
            !string.IsNullOrEmpty(x.value?.ToString())
            && !await _repository.NameExistAsync(x.value?.ToString()),
            "The department name already exist."
          },
        });

      #endregion

      #region ShortName

      AddFailureForPropertyIf(
        nameof(EditDepartmentRequest.ShortName),
        x => x == OperationType.Replace,
        new()
        {
          { x => !string.IsNullOrEmpty(x.value?.ToString().Trim()), "Short name must not be empty." },
          { x => x.value.ToString().Trim().Length > 2, "Short name is too short." },
          { x => x.value.ToString().Length < 41, "Short name is too long." },
        }, CascadeMode.Stop);

      await AddFailureForPropertyIfAsync(
        nameof(EditDepartmentRequest.ShortName),
        x => x == OperationType.Replace,
        new()
        {
          {
            async x =>
            !string.IsNullOrEmpty(x.value?.ToString())
            && !await _repository.ShortNameExistAsync(x.value?.ToString()),
            "The department short name already exist."
          },
        });

      #endregion

      #region Description

      AddFailureForPropertyIf(
        nameof(EditDepartmentRequest.Description),
        x => x == OperationType.Replace,
        new()
        {
          { x => x.value?.ToString().Trim().Length < 1000, "Description is too long." },
        });

      #endregion

      #region IsActive

      AddFailureForPropertyIf(
        nameof(EditDepartmentRequest.IsActive),
        x => x == OperationType.Replace,
        new()
        {
          { x => bool.TryParse(x.value?.ToString(), out bool _), "Incorrect format of IsActive." },
        });

      #endregion
    }

    public EditDepartmentRequestValidator(
      IDepartmentRepository repository)
    {
      _repository = repository;

      RuleForEach(request => request.Operations)
        .CustomAsync(async (x, context, token) => await HandleInternalPropertyValidation(x, context));
    }
  }
}
