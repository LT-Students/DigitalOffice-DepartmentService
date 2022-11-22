using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Validators;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department;
using LT.DigitalOffice.DepartmentService.Validation.Department.Interfaces;
using LT.DigitalOffice.DepartmentService.Validation.Department.Resources;
using LT.DigitalOffice.Kernel.Validators;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace LT.DigitalOffice.DepartmentService.Validation.Department
{
  public class EditDepartmentRequestValidator : ExtendedEditRequestValidator<Guid, EditDepartmentRequest>, IEditDepartmentRequestValidator
  {
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ICategoryRepository _categoryRepository;

    private async Task HandleInternalPropertyValidationAsync(
      Operation<EditDepartmentRequest> requestedOperation,
      Guid departmentId,
      ValidationContext<(Guid, JsonPatchDocument<EditDepartmentRequest>)> context)
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
          nameof(EditDepartmentRequest.IsActive),
          nameof(EditDepartmentRequest.CategoryId)
        });

      AddСorrectOperations(nameof(EditDepartmentRequest.Name), new() { OperationType.Replace });
      AddСorrectOperations(nameof(EditDepartmentRequest.ShortName), new() { OperationType.Replace });
      AddСorrectOperations(nameof(EditDepartmentRequest.Description), new() { OperationType.Replace });
      AddСorrectOperations(nameof(EditDepartmentRequest.IsActive), new() { OperationType.Replace });
      AddСorrectOperations(nameof(EditDepartmentRequest.CategoryId), new() { OperationType.Replace });

      #endregion

      #region Name

      AddFailureForPropertyIf(
        nameof(EditDepartmentRequest.Name),
        x => x == OperationType.Replace,
        new()
        {
          { x => !string.IsNullOrEmpty(x.value?.ToString().Trim()), EditDepartmentRequestValidatorResource.EmptyName },
          { x => x.value.ToString().Trim().Length > 2, EditDepartmentRequestValidatorResource.NameTooShort },
          { x => x.value.ToString().Length < 300, EditDepartmentRequestValidatorResource.NameTooLong },
        }, CascadeMode.Stop);

      await AddFailureForPropertyIfAsync(
        nameof(EditDepartmentRequest.Name),
        x => x == OperationType.Replace,
        new()
        {
          {
            async x =>
            !string.IsNullOrEmpty(x.value?.ToString())
            && !await _departmentRepository.NameExistAsync(x.value?.ToString(), departmentId),
            EditDepartmentRequestValidatorResource.ExistingName
          },
        });

      #endregion

      #region ShortName

      AddFailureForPropertyIf(
        nameof(EditDepartmentRequest.ShortName),
        x => x == OperationType.Replace,
        new()
        {
          { x => !string.IsNullOrEmpty(x.value?.ToString().Trim()), EditDepartmentRequestValidatorResource.EmptyShortName },
          { x => x.value.ToString().Trim().Length > 2, EditDepartmentRequestValidatorResource.ShortNameTooShort },
          { x => x.value.ToString().Length < 41, EditDepartmentRequestValidatorResource.ShortNameTooLong },
        }, CascadeMode.Stop);

      await AddFailureForPropertyIfAsync(
        nameof(EditDepartmentRequest.ShortName),
        x => x == OperationType.Replace,
        new()
        {
          {
            async x =>
            !string.IsNullOrEmpty(x.value?.ToString())
            && !await _departmentRepository.ShortNameExistAsync(x.value?.ToString(), departmentId),
            EditDepartmentRequestValidatorResource.ExistingShortName
          },
        });

      #endregion

      #region Description

      AddFailureForPropertyIf(
        nameof(EditDepartmentRequest.Description),
        x => x == OperationType.Replace,
        new()
        {
          { x => x.value?.ToString().Trim().Length < 1000, EditDepartmentRequestValidatorResource.DescriptionTooLong },
        });

      #endregion

      #region IsActive

      AddFailureForPropertyIf(
        nameof(EditDepartmentRequest.IsActive),
        x => x == OperationType.Replace,
        new()
        {
          { x => bool.TryParse(x.value?.ToString(), out bool _), EditDepartmentRequestValidatorResource.IncorrectIsActiveFormat },
        });

      #endregion

      #region CategoryId

      await AddFailureForPropertyIfAsync(
        nameof(EditDepartmentRequest.CategoryId),
        x => x == OperationType.Replace,
        new()
        {
          {
            async (x) =>
            {
              if (x.value?.ToString() is null)
              {
                return true;
              }

              return Guid.TryParse(x.value.ToString(), out Guid categoryId)
                ? await _categoryRepository.IdExistAsync(categoryId)
                : false;
            },
            EditDepartmentRequestValidatorResource.NotExistingCategory
          }
        });
    }

      #endregion

    public EditDepartmentRequestValidator(
      IDepartmentRepository departmentRepository,
      ICategoryRepository categoryRepository)
    {
      _departmentRepository = departmentRepository;
      _categoryRepository = categoryRepository;

      RuleFor(x => x)
        .CustomAsync(async (x, context, _) =>
        {
          foreach (var op in x.Item2.Operations)
          {
            await HandleInternalPropertyValidationAsync(op, x.Item1, context);
          }
        });
    }
  }
}
