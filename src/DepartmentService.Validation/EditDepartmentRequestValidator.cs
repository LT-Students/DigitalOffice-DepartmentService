using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Validators;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.DepartmentService.Validation.Interfaces;
using LT.DigitalOffice.Kernel.Validators;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace LT.DigitalOffice.DepartmentService.Validation.Department
{
  public class EditDepartmentRequestValidator : BaseEditRequestValidator<EditDepartmentRequest>, IEditDepartmentRequestValidator
  {
    private async Task HandleInternalPropertyValidation(Operation<EditDepartmentRequest> requestedOperation, CustomContext context)
    {
      RequestedOperation = requestedOperation;
      Context = context;

      #region Paths

      AddСorrectPaths(
        new List<string>
        {
          nameof(EditDepartmentRequest.Name),
          nameof(EditDepartmentRequest.Description),
          nameof(EditDepartmentRequest.DirectorId),
          nameof(EditDepartmentRequest.IsActive)
        });

      AddСorrectOperations(nameof(EditDepartmentRequest.Name), new() { OperationType.Replace });
      AddСorrectOperations(nameof(EditDepartmentRequest.Description), new() { OperationType.Replace });
      AddСorrectOperations(nameof(EditDepartmentRequest.DirectorId), new() { OperationType.Replace });
      AddСorrectOperations(nameof(EditDepartmentRequest.IsActive), new() { OperationType.Replace });

      #endregion

      #region Name

      AddFailureForPropertyIf(
        nameof(EditDepartmentRequest.Name),
        x => x == OperationType.Replace,
        new()
        {
          { x => !string.IsNullOrEmpty(x.value?.ToString().Trim()), "Name must not be empty." },
          { x => x.value.ToString().Trim().Length < 100, "Name is too long." },
          { x => x.value.ToString().Trim().Length > 2, "Name is too short." },
        }, CascadeMode.Stop);

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

      #region DirectorId

      AddFailureForPropertyIf(
        nameof(EditDepartmentRequest.DirectorId),
        x => x == OperationType.Replace,
        new()
        {
          { x => Guid.TryParse(x.value.ToString(), out Guid _), "Incorrect format of DirectorId." },
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

    public EditDepartmentRequestValidator()
    {
      RuleForEach(x => x.Operations)
        .CustomAsync(async (x, context, token) => await HandleInternalPropertyValidation(x, context));
    }
  }
}
