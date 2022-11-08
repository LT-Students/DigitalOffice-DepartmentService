using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.Department;
using LT.DigitalOffice.DepartmentService.Business.Department.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Patch.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department;
using LT.DigitalOffice.DepartmentService.Validation.Department.Interfaces;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.UnitTestKernel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Moq;
using Moq.AutoMock;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace LT.DigitalOffice.DepartmentService.Business.UnitTests.Commands.Department;

public class EditDepartmentCommandTests
{
  private AutoMocker _autoMocker;
  private IEditDepartmentCommand _command;

  private JsonPatchDocument<EditDepartmentRequest> _request;
  private JsonPatchDocument<DbDepartment> _dbRequest;

  [OneTimeSetUp]
  public void OneTimeSetUp()
  {
    _autoMocker = new();

    _autoMocker
      .Setup<IHttpContextAccessor, int>(a => a.HttpContext.Response.StatusCode)
      .Returns(200);

    _autoMocker
      .Setup<IResponseCreator, OperationResultResponse<bool>>(
        x => x.CreateFailureResponse<bool>(HttpStatusCode.BadRequest, It.IsAny<List<string>>()))
      .Returns(new OperationResultResponse<bool>()
      {
        Errors = new() { "Request is not correct." }
      });

    _autoMocker
      .Setup<IResponseCreator, OperationResultResponse<bool>>(
        x => x.CreateFailureResponse<bool>(HttpStatusCode.Forbidden, It.IsAny<List<string>>()))
      .Returns(new OperationResultResponse<bool>()
      {
        Errors = new() { "Not enough rights." }
      });

    _command = _autoMocker.CreateInstance<EditDepartmentCommand>();

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

    _dbRequest = new JsonPatchDocument<DbDepartment>(
      new List<Operation<DbDepartment>>
      {
        new Operation<DbDepartment>(
          "replace",
          $"/{nameof(DbDepartment.Name)}",
          "",
          "New Name"),

        new Operation<DbDepartment>(
          "replace",
          $"/{nameof(DbDepartment.ShortName)}",
          "",
          "new ShortName"),

        new Operation<DbDepartment>(
          "replace",
          $"/{nameof(DbDepartment.Description)}",
          "",
          "new Description"),

        new Operation<DbDepartment>(
          "replace",
          $"/{nameof(DbDepartment.IsActive)}",
          "",
          true),

        new Operation<DbDepartment>(
          "replace",
          $"/{nameof(DbDepartment.CategoryId)}",
          "",
          Guid.NewGuid())
      }, new CamelCasePropertyNamesContractResolver());
  }

  [SetUp]
  public void Setup()
  {
    _autoMocker.GetMock<IAccessValidator>().Reset();
    _autoMocker.GetMock<IEditDepartmentRequestValidator>().Reset();
    _autoMocker.GetMock<IPatchDbDepartmentMapper>().Reset();
    _autoMocker.GetMock<IDepartmentRepository>().Reset();

    _autoMocker
      .Setup<IAccessValidator, Task<bool>>(x => x.HasRightsAsync(Rights.AddEditRemoveDepartments))
      .ReturnsAsync(true);

    _autoMocker
      .Setup<IEditDepartmentRequestValidator, bool>(x => x.ValidateAsync(It.IsAny<ValueTuple<Guid, JsonPatchDocument<EditDepartmentRequest>>>(), default).Result.IsValid)
      .Returns(true);

    _autoMocker
      .Setup<IPatchDbDepartmentMapper, JsonPatchDocument<DbDepartment>>(x => x.Map(_request))
      .Returns(_dbRequest);

    _autoMocker
      .Setup<IDepartmentRepository, Task<bool>>(x => x.EditAsync(It.IsAny<Guid>(), It.IsAny<JsonPatchDocument<DbDepartment>>()))
      .Returns(Task.FromResult(true));
  }

  [Test]
  public async Task ShouldReturnFailedResponseWhenUserHasNotRightAsync()
  {
    _autoMocker
     .Setup<IAccessValidator, Task<bool>>(x => x.HasRightsAsync(Rights.AddEditRemoveDepartments))
     .ReturnsAsync(false);

    OperationResultResponse<bool> expectedResponse = new()
    {
      Errors = new List<string> { "Not enough rights." }
    };

    SerializerAssert.AreEqual(expectedResponse, await _command.ExecuteAsync(Guid.NewGuid(), _request));

    _autoMocker.Verify<IAccessValidator>(
      x => x.HasRightsAsync(It.IsAny<int>()),
      Times.Once);

    _autoMocker.Verify<IEditDepartmentRequestValidator>(
      x => x.ValidateAsync(It.IsAny<ValueTuple<Guid, JsonPatchDocument<EditDepartmentRequest>>>(), It.IsAny<CancellationToken>()),
      Times.Never);

    _autoMocker.Verify<IPatchDbDepartmentMapper>(
      x => x.Map(It.IsAny<JsonPatchDocument<EditDepartmentRequest>>()),
      Times.Never);

    _autoMocker.Verify<IDepartmentRepository>(
      x => x.EditAsync(It.IsAny<Guid>(), It.IsAny<JsonPatchDocument<DbDepartment>>()),
      Times.Never);
  }

  [Test]
  public async Task ShouldReturnFailedResponseWhenValidationIsFailedAsync()
  {

    _autoMocker
      .Setup<IEditDepartmentRequestValidator, bool>(x => x.ValidateAsync(It.IsAny<ValueTuple<Guid, JsonPatchDocument<EditDepartmentRequest>>>(), default).Result.IsValid)
      .Returns(false);

    OperationResultResponse<bool> expectedResponse = new()
    {
      Errors = new List<string> { "Request is not correct." }
    };

    SerializerAssert.AreEqual(expectedResponse, await _command.ExecuteAsync(Guid.NewGuid(), _request));

    _autoMocker.Verify<IAccessValidator>(
      x => x.HasRightsAsync(It.IsAny<int>()),
      Times.Once);

    _autoMocker.Verify<IEditDepartmentRequestValidator>(
      x => x.ValidateAsync(It.IsAny<ValueTuple<Guid, JsonPatchDocument<EditDepartmentRequest>>>(), It.IsAny<CancellationToken>()),
      Times.Once);

    _autoMocker.Verify<IPatchDbDepartmentMapper>(
      x => x.Map(It.IsAny<JsonPatchDocument<EditDepartmentRequest>>()),
      Times.Never);

    _autoMocker.Verify<IDepartmentRepository>(
      x => x.EditAsync(It.IsAny<Guid>(), It.IsAny<JsonPatchDocument<DbDepartment>>()),
      Times.Never);
  }

  [Test]
  public async Task ShouldEditDepartmentSuccesfullAsync()
  {
    OperationResultResponse<bool> expectedResponse = new()
    {
      Body = true
    };

    SerializerAssert.AreEqual(expectedResponse, await _command.ExecuteAsync(Guid.NewGuid(), _request));

    _autoMocker.Verify<IAccessValidator>(
       x => x.HasRightsAsync(It.IsAny<int>()),
       Times.Once);

    _autoMocker.Verify<IEditDepartmentRequestValidator>(
      x => x.ValidateAsync(It.IsAny<ValueTuple<Guid, JsonPatchDocument<EditDepartmentRequest>>>(), It.IsAny<CancellationToken>()),
      Times.Once);

    _autoMocker.Verify<IPatchDbDepartmentMapper>(
      x => x.Map(It.IsAny<JsonPatchDocument<EditDepartmentRequest>>()),
      Times.Once);

    _autoMocker.Verify<IDepartmentRepository>(
      x => x.EditAsync(It.IsAny<Guid>(), It.IsAny<JsonPatchDocument<DbDepartment>>()),
      Times.Once);
  }
}
