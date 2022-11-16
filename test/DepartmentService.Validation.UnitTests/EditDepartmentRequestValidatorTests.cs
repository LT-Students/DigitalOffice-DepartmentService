using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation.TestHelper;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department;
using LT.DigitalOffice.DepartmentService.Validation.Department;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Moq;
using Moq.AutoMock;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace LT.DigitalOffice.DepartmentService.Validation.UnitTests;

public class EditDepartmentRequestValidatorTestss
{
  private AutoMocker _autoMocker;
  private EditDepartmentRequestValidator _validator;
  private JsonPatchDocument<EditDepartmentRequest> _request;
  private Guid _departmentId;

  Func<string, Operation> GetOperationByPath =>
    (path) => _request.Operations.Find(x => x.path.EndsWith(path, StringComparison.OrdinalIgnoreCase));

  private void Verifiable(
  Times departmentRepositoryNameExistTimes,
  Times departmentRepositoryShortNameExistTimes,
  Times categoryRepositoryIdExistTimes)
  {
    _autoMocker.Verify<IDepartmentRepository, Task<bool>>(x => x.NameExistAsync(It.IsAny<string>(), It.IsAny<Guid>()), departmentRepositoryNameExistTimes);
    _autoMocker.Verify<IDepartmentRepository, Task<bool>>(x => x.ShortNameExistAsync(It.IsAny<string>(), It.IsAny<Guid>()), departmentRepositoryShortNameExistTimes);
    _autoMocker.Verify<ICategoryRepository, Task<bool>>(x => x.IdExistAsync(It.IsAny<Guid>()), categoryRepositoryIdExistTimes);

    _autoMocker.Resolvers.Clear();
  }

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

    _autoMocker
      .Setup<IDepartmentRepository, Task<bool>>(x => x.NameExistAsync(It.IsAny<string>(), It.IsAny<Guid>()))
      .ReturnsAsync(false);

    _autoMocker
      .Setup<IDepartmentRepository, Task<bool>>(x => x.ShortNameExistAsync(It.IsAny<string>(), It.IsAny<Guid>()))
      .ReturnsAsync(false);

    _autoMocker
      .Setup<ICategoryRepository, Task<bool>>(x => x.IdExistAsync(It.IsAny<Guid>()))
      .ReturnsAsync(true);

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
  public async Task SuccessValidationTestAsync()
  {
    (await _validator.TestValidateAsync((_departmentId, _request))).ShouldNotHaveAnyValidationErrors();

    Verifiable(
      departmentRepositoryNameExistTimes: Times.Exactly(2),
      departmentRepositoryShortNameExistTimes: Times.Once(),
      categoryRepositoryIdExistTimes: Times.Once());
  }

  [Test]
  public async Task NameIsEmptyTestAsync()
  {
    GetOperationByPath(nameof(EditDepartmentRequest.Name)).value = string.Empty;

    (await _validator.TestValidateAsync((_departmentId, _request))).ShouldHaveAnyValidationError();

    Verifiable(
      departmentRepositoryNameExistTimes: Times.Once(),
      departmentRepositoryShortNameExistTimes: Times.Once(),
      categoryRepositoryIdExistTimes: Times.Once());
  }

  [Test]
  public async Task NameIsTooLongTestAsync()
  {
    GetOperationByPath(nameof(EditDepartmentRequest.Name)).value = new string('*', 300);

    (await _validator.TestValidateAsync((_departmentId, _request))).ShouldHaveAnyValidationError();

    Verifiable(
      departmentRepositoryNameExistTimes: Times.Exactly(2),
      departmentRepositoryShortNameExistTimes: Times.Once(),
      categoryRepositoryIdExistTimes: Times.Once());
  }

  [Test]
  public async Task NameIsTooShortTestAsync()
  {
    GetOperationByPath(nameof(EditDepartmentRequest.Name)).value = "*";

    (await _validator.TestValidateAsync((_departmentId, _request))).ShouldHaveAnyValidationError();

    Verifiable(
      departmentRepositoryNameExistTimes: Times.Exactly(2),
      departmentRepositoryShortNameExistTimes: Times.Once(),
      categoryRepositoryIdExistTimes: Times.Once());
  }

  [Test]
  public async Task NameAlreadyExistsTestAsync()
  {
    _autoMocker
      .Setup<IDepartmentRepository, Task<bool>>(x => x.NameExistAsync(It.IsAny<String>(), It.IsAny<Guid>()))
      .ReturnsAsync(true);

    (await _validator.TestValidateAsync((_departmentId, _request))).ShouldHaveAnyValidationError();

    Verifiable(
      departmentRepositoryNameExistTimes: Times.Exactly(2),
      departmentRepositoryShortNameExistTimes: Times.Once(),
      categoryRepositoryIdExistTimes: Times.Once());
  }

  [Test]
  public async Task ShortNameIsTooShortTestAsync()
  {
    GetOperationByPath(nameof(EditDepartmentRequest.ShortName)).value = "*";

    (await _validator.TestValidateAsync((_departmentId, _request))).ShouldHaveAnyValidationError();

    Verifiable(
      departmentRepositoryNameExistTimes: Times.Exactly(2),
      departmentRepositoryShortNameExistTimes: Times.Once(),
      categoryRepositoryIdExistTimes: Times.Once());
  }

  [Test]
  public async Task ShortNameIsTooLongTestAsync()
  {
    GetOperationByPath(nameof(EditDepartmentRequest.ShortName)).value = new String('*', 41);

    (await _validator.TestValidateAsync((_departmentId, _request))).ShouldHaveAnyValidationError();

    Verifiable(
      departmentRepositoryNameExistTimes: Times.Exactly(2),
      departmentRepositoryShortNameExistTimes: Times.Once(),
      categoryRepositoryIdExistTimes: Times.Once());
  }

  [Test]
  public async Task ShortNameIsEmptyTestAsync()
  {
    GetOperationByPath(nameof(EditDepartmentRequest.ShortName)).value = String.Empty;

    (await _validator.TestValidateAsync((_departmentId, _request))).ShouldHaveAnyValidationError();

    Verifiable(
      departmentRepositoryNameExistTimes: Times.Once(),
      departmentRepositoryShortNameExistTimes: Times.Never(),
      categoryRepositoryIdExistTimes: Times.Once());
  }

  [Test]
  public async Task ShortNameAlreadyExistsTestAsync()
  {
    _autoMocker
      .Setup<IDepartmentRepository, Task<bool>>(x => x.ShortNameExistAsync(It.IsAny<String>(), It.IsAny<Guid>()))
      .ReturnsAsync(true);

    (await _validator.TestValidateAsync((_departmentId, _request))).ShouldHaveAnyValidationError();

    Verifiable(
      departmentRepositoryNameExistTimes: Times.Exactly(2),
      departmentRepositoryShortNameExistTimes: Times.Once(),
      categoryRepositoryIdExistTimes: Times.Once());
  }

  [Test]
  public async Task DescriptionIsTooLongTestAsync()
  {
    GetOperationByPath(nameof(EditDepartmentRequest.Description)).value = new String('*', 1000);

    (await _validator.TestValidateAsync((_departmentId, _request))).ShouldHaveAnyValidationError();

    Verifiable(
      departmentRepositoryNameExistTimes: Times.Exactly(2),
      departmentRepositoryShortNameExistTimes: Times.Once(),
      categoryRepositoryIdExistTimes: Times.Once());
  }

  [Test]
  public async Task CategoryIdIsNullTestAsync()
  {
    GetOperationByPath(nameof(EditDepartmentRequest.CategoryId)).value = null;

    (await _validator.TestValidateAsync((_departmentId, _request))).ShouldNotHaveAnyValidationErrors();

    Verifiable(
      departmentRepositoryNameExistTimes: Times.Exactly(2),
      departmentRepositoryShortNameExistTimes: Times.Once(),
      categoryRepositoryIdExistTimes: Times.Never());
  }

  [Test]
  public async Task CategoryIdDoesNotExistTestAsync()
  {
    _autoMocker
      .Setup<ICategoryRepository, Task<bool>>(x => x.IdExistAsync(It.IsAny<Guid>()))
      .ReturnsAsync(false);

    (await _validator.TestValidateAsync((_departmentId, _request))).ShouldHaveAnyValidationError();

    Verifiable(
      departmentRepositoryNameExistTimes: Times.Exactly(2),
      departmentRepositoryShortNameExistTimes: Times.Once(),
      categoryRepositoryIdExistTimes: Times.Once());
  }

  [Test]
  public async Task CategoryIdIsNotParsedTestAsync()
  {
    GetOperationByPath(nameof(EditDepartmentRequest.CategoryId)).value = "*";

    (await _validator.TestValidateAsync((_departmentId, _request))).ShouldHaveAnyValidationError();

    Verifiable(
      departmentRepositoryNameExistTimes: Times.Exactly(2),
      departmentRepositoryShortNameExistTimes: Times.Once(),
      categoryRepositoryIdExistTimes: Times.Never());
  }

  [Test]
  public async Task IsActiveHasWrongFormatTestAsync()
  {
    GetOperationByPath(nameof(EditDepartmentRequest.IsActive)).value = "*";

    (await _validator.TestValidateAsync((_departmentId, _request))).ShouldHaveAnyValidationError();

    Verifiable(
      departmentRepositoryNameExistTimes: Times.Exactly(2),
      departmentRepositoryShortNameExistTimes: Times.Once(),
      categoryRepositoryIdExistTimes: Times.Once());
  }
}
