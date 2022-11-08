using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department;
using LT.DigitalOffice.DepartmentService.Validation.Department;
using LT.DigitalOffice.DepartmentService.Validation.Department.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Moq;
using Moq.AutoMock;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace LT.DigitalOffice.DepartmentService.Validation.UnitTests;

public class EditDepartmentRequestValidatorTest
{
  private AutoMocker _autoMocker;
  private EditDepartmentRequestValidator _validator;
  private JsonPatchDocument<EditDepartmentRequest> _request;
  private Guid _departmentId;

  Func<string, Operation> GetOperationByPath =>
    (path) => _request.Operations.Find(x => x.path.EndsWith(path, StringComparison.OrdinalIgnoreCase));

  [OneTimeSetUp]
  public void OneTimeSetUp()
  {
    _departmentId = Guid.NewGuid();
    _autoMocker = new AutoMocker();
    _validator = _autoMocker.CreateInstance<EditDepartmentRequestValidator>();
  }

  [SetUp]
  public void SetUp()
  {
    _autoMocker.GetMock<IDepartmentRepository>().Reset();
    _autoMocker.GetMock<ICategoryRepository>().Reset();
    _autoMocker.GetMock<IEditDepartmentRequestValidator>().Reset();

    _autoMocker
      .Setup<IDepartmentRepository, Task<bool>>(x => x.NameExistAsync(It.IsAny<String>(), It.IsAny<Guid>()))
      .Returns(Task.FromResult(false));

    _autoMocker
      .Setup<IDepartmentRepository, Task<bool>>(x => x.ShortNameExistAsync(It.IsAny<String>(), It.IsAny<Guid>()))
      .Returns(Task.FromResult(false));

    _autoMocker
      .Setup<ICategoryRepository, Task<bool>>(x => x.IdExistAsync(It.IsAny<Guid>()))
      .Returns(Task.FromResult(true));

    _request = new JsonPatchDocument<EditDepartmentRequest>(
      new List<Operation<EditDepartmentRequest>>
      {
        new Operation<EditDepartmentRequest>(
          "replace",
          $"/{nameof(EditDepartmentRequest.Name)}",
          "",
          "new Name"),

        new Operation<EditDepartmentRequest>(
          "replace",
          $"/{nameof(EditDepartmentRequest.ShortName)}",
          "",
          "new ShortName"),

        new Operation<EditDepartmentRequest>(
          "replace",
          $"/{nameof(EditDepartmentRequest.Description)}",
          "",
          "new Description"),

        new Operation<EditDepartmentRequest>(
          "replace",
          $"/{nameof(EditDepartmentRequest.IsActive)}",
          "",
          true),

        new Operation<EditDepartmentRequest>(
          "replace",
          $"/{nameof(EditDepartmentRequest.CategoryId)}",
          "",
          Guid.NewGuid())
      }, new CamelCasePropertyNamesContractResolver());
  }

  [Test]
  public void SuccessValidation()
  {
    Assert.True(_validator.ValidateAsync((_departmentId, _request)).Result.IsValid);
  }

  [Test]
  public void ShouldThrowExceptionWhenNameIsEmpty()
  {
    GetOperationByPath(nameof(EditDepartmentRequest.Name)).value = String.Empty;

    Assert.False(_validator.ValidateAsync((_departmentId, _request)).Result.IsValid);
  }

  [Test]
  public void ShouldThrowExceptionWhenNameIsTooLong()
  {
    GetOperationByPath(nameof(EditDepartmentRequest.Name)).value = new String('*', 300);

    Assert.False(_validator.ValidateAsync((_departmentId, _request)).Result.IsValid);
  }

  [Test]
  public void ShouldThrowExceptionWhenNameIsTooShort()
  {
    GetOperationByPath(nameof(EditDepartmentRequest.Name)).value = "*";

    Assert.False(_validator.ValidateAsync((_departmentId, _request)).Result.IsValid);
  }

  [Test]
  public void ShouldThrowExceptionWhenNameAlreadyExists()
  {
    _autoMocker
      .Setup<IDepartmentRepository, Task<bool>>(x => x.NameExistAsync(It.IsAny<String>(), It.IsAny<Guid>()))
      .Returns(Task.FromResult(true));

    Assert.False(_validator.ValidateAsync((_departmentId, _request)).Result.IsValid);
  }

  [Test]
  public void ShouldThrowExceptionWhenShortNameIsTooShort()
  {
    GetOperationByPath(nameof(EditDepartmentRequest.ShortName)).value = "*";

    Assert.False(_validator.ValidateAsync((_departmentId, _request)).Result.IsValid);
  }

  [Test]
  public void ShouldThrowExceptionWhenShortNameIsTooLong()
  {
    GetOperationByPath(nameof(EditDepartmentRequest.ShortName)).value = new String('*', 41);

    Assert.False(_validator.ValidateAsync((_departmentId, _request)).Result.IsValid);
  }

  [Test]
  public void ShouldThrowExceptionWhenShortNameIsEmpty()
  {
    GetOperationByPath(nameof(EditDepartmentRequest.ShortName)).value = String.Empty;

    Assert.False(_validator.ValidateAsync((_departmentId, _request)).Result.IsValid);
  }

  [Test]
  public void ShouldThrowExceptionWhenShortNameAlreadyExists()
  {
    _autoMocker
      .Setup<IDepartmentRepository, Task<bool>>(x => x.ShortNameExistAsync(It.IsAny<String>(), It.IsAny<Guid>()))
      .Returns(Task.FromResult(true));

    Assert.False(_validator.ValidateAsync((_departmentId, _request)).Result.IsValid);
  }

  [Test]
  public void ShouldThrowExceptionWhenDescriptionIsTooLong()
  {
    GetOperationByPath(nameof(EditDepartmentRequest.Description)).value = new String('*', 1000);

    Assert.False(_validator.ValidateAsync((_departmentId, _request)).Result.IsValid);
  }

  [Test]
  public void ShouldNotThrowExceptionWhenCategoryIdIsNull()
  {
    GetOperationByPath(nameof(EditDepartmentRequest.CategoryId)).value = null;

    Assert.True(_validator.ValidateAsync((_departmentId, _request)).Result.IsValid);
  }

  [Test]
  public void ShouldThrowExceptionWhenCategoryIdDoesNotExist()
  {
    _autoMocker
      .Setup<ICategoryRepository, Task<bool>>(x => x.IdExistAsync(It.IsAny<Guid>()))
      .Returns(Task.FromResult(false));

    Assert.False(_validator.ValidateAsync((_departmentId, _request)).Result.IsValid);
  }

  [Test]
  public void ShouldThrowExceptionWhenCategoryIdIsNotParsed()
  {
    GetOperationByPath(nameof(EditDepartmentRequest.CategoryId)).value = "*";

    Assert.False(_validator.ValidateAsync((_departmentId, _request)).Result.IsValid);
  }

  [Test]
  public void ShouldThrowExceptionWhenIsActiveHasWrongFormat()
  {
    GetOperationByPath(nameof(EditDepartmentRequest.IsActive)).value = "*";

    Assert.False(_validator.ValidateAsync((_departmentId, _request)).Result.IsValid);
  }
}
