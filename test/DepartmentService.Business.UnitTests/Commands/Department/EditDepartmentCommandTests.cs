using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using LT.DigitalOffice.DepartmentService.Broker.Helpers.Branch.Interfaces;
using LT.DigitalOffice.DepartmentService.Broker.Helpers.MemoryCache.Interfaces;
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
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.UnitTestKernel;
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

  private void Verifiable(
    Times accessValidatorTimes,
    Times editDepartmentRequestValidatorTimes,
    Times departmentRepositoryEditTimes,
    Times patchDbDepartmentMapperTimes,
    Times responseCreatorTimes,
    Times departmentUserRepositoryTimes,
    Times memoryCacheHelperRemoveTimes,
    Times memoryCacheHelperGetDepartmentsTreeTimes,
    Times departmentRepositoryRemoveTimes,
    Times globalCacheRepositoryTimes,
    Times departmentBranchHelperTimes)
  {
    _autoMocker.Verify<IAccessValidator, Task<bool>>(x => x.HasRightsAsync(It.IsAny<int>()), accessValidatorTimes);
    _autoMocker.Verify<IEditDepartmentRequestValidator>(
      x => x.ValidateAsync(It.IsAny<ValueTuple<Guid, JsonPatchDocument<EditDepartmentRequest>>>(), It.IsAny<CancellationToken>()), editDepartmentRequestValidatorTimes);
    _autoMocker.Verify<IPatchDbDepartmentMapper, JsonPatchDocument<DbDepartment>>(x => x.Map(_request), patchDbDepartmentMapperTimes);
    _autoMocker.Verify<IDepartmentRepository, Task<bool>>(x => x.EditAsync(It.IsAny<Guid>(), It.IsAny<JsonPatchDocument<DbDepartment>>()), departmentRepositoryEditTimes);
    _autoMocker.Verify<IResponseCreator, OperationResultResponse<bool>>(
      x => x.CreateFailureResponse<bool>(It.IsAny<HttpStatusCode>(), It.IsAny<List<string>>()), responseCreatorTimes);
    _autoMocker.Verify<IDepartmentUserRepository, Task>(x => x.RemoveAsync(It.IsAny<List<Guid>>()), departmentUserRepositoryTimes);
    _autoMocker.Verify<IDepartmentRepository, Task>(x => x.RemoveAsync(It.IsAny<List<Guid>>()), departmentRepositoryRemoveTimes);
    _autoMocker.Verify<IMemoryCacheHelper>(x => x.Remove(It.IsAny<string[]>()), memoryCacheHelperRemoveTimes);
    _autoMocker.Verify<IMemoryCacheHelper, Task<List<Tuple<Guid, string, string, Guid?>>>>(x => x.GetDepartmentsTreeAsync(), memoryCacheHelperGetDepartmentsTreeTimes);
    _autoMocker.Verify<IGlobalCacheRepository, Task>(x => x.RemoveAsync(It.IsAny<Guid>()), globalCacheRepositoryTimes);
    _autoMocker.Verify<IDepartmentBranchHelper, List<Guid>>(
      x => x.GetChildrenIds(It.IsAny<List<Tuple<Guid, string, string, Guid?>>>(), It.IsAny<Guid>()), departmentBranchHelperTimes);

    _autoMocker.Resolvers.Clear();
  }


  [OneTimeSetUp]
  public void OneTimeSetUp()
  {
    _autoMocker = new();

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
          true)

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
    _autoMocker.GetMock<IResponseCreator>().Reset();
    _autoMocker.GetMock<IDepartmentUserRepository>().Reset();
    _autoMocker.GetMock<IMemoryCacheHelper>().Reset();
    _autoMocker.GetMock<IGlobalCacheRepository>().Reset();
    _autoMocker.GetMock<IDepartmentBranchHelper>().Reset();

    _autoMocker
      .Setup<IDepartmentBranchHelper, List<Guid>>(x => x.GetChildrenIds(It.IsAny<List<Tuple<Guid, string, string, Guid?>>>(), It.IsAny<Guid>()))
      .Returns(new List<Guid>());

    _autoMocker
      .Setup<IMemoryCacheHelper, Task<List<Tuple<Guid, string, string, Guid?>>>>(x => x.GetDepartmentsTreeAsync())
      .ReturnsAsync(new List<Tuple<Guid, string, string, Guid?>>());

    _autoMocker
      .Setup<IGlobalCacheRepository, Task>(x => x.RemoveAsync(It.IsAny<Guid>()));

    _autoMocker
      .Setup<IMemoryCacheHelper>(x => x.Remove(It.IsAny<string[]>()));

    _autoMocker
      .Setup<IDepartmentUserRepository, Task>(x => x.RemoveAsync(It.IsAny<List<Guid>>()));

    _autoMocker
      .Setup<IDepartmentRepository, Task>(x => x.RemoveAsync(It.IsAny<List<Guid>>()));

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

    _autoMocker
      .Setup<IAccessValidator, Task<bool>>(x => x.HasRightsAsync(Rights.AddEditRemoveDepartments))
      .ReturnsAsync(true);

    _autoMocker
      .Setup<IEditDepartmentRequestValidator, Task<ValidationResult>>(x => x.ValidateAsync(It.IsAny<ValueTuple<Guid, JsonPatchDocument<EditDepartmentRequest>>>(), default))
      .ReturnsAsync(new ValidationResult());

    _autoMocker
      .Setup<IPatchDbDepartmentMapper, JsonPatchDocument<DbDepartment>>(x => x.Map(_request))
      .Returns(_dbRequest);

    _autoMocker
      .Setup<IDepartmentRepository, Task<bool>>(x => x.EditAsync(It.IsAny<Guid>(), It.IsAny<JsonPatchDocument<DbDepartment>>()))
      .ReturnsAsync(true);

    _request.Operations.Find(x => x.path.EndsWith(nameof(EditDepartmentRequest.IsActive), StringComparison.OrdinalIgnoreCase)).value = true;
  }

  [Test]
  public async Task NotEnoughRightTestAsync()
  {
    _autoMocker
     .Setup<IAccessValidator, Task<bool>>(x => x.HasRightsAsync(Rights.AddEditRemoveDepartments))
     .ReturnsAsync(false);

    OperationResultResponse<bool> expectedResponse = new()
    {
      Errors = new List<string> { "Not enough rights." }
    };

    SerializerAssert.AreEqual(expectedResponse, await _command.ExecuteAsync(Guid.NewGuid(), _request));

    Verifiable(
      accessValidatorTimes: Times.Once(),
      responseCreatorTimes: Times.Once(),
      editDepartmentRequestValidatorTimes: Times.Never(),
      departmentRepositoryEditTimes: Times.Never(),
      patchDbDepartmentMapperTimes: Times.Never(),
      departmentUserRepositoryTimes: Times.Never(),
      memoryCacheHelperRemoveTimes: Times.Never(),
      memoryCacheHelperGetDepartmentsTreeTimes: Times.Never(),
      departmentRepositoryRemoveTimes: Times.Never(),
      globalCacheRepositoryTimes: Times.Never(),
      departmentBranchHelperTimes: Times.Never());
  }

  [Test]
  public async Task ValidationFailureTestAsync()
  {
    _autoMocker
      .Setup<IEditDepartmentRequestValidator, Task<ValidationResult>>(x => x.ValidateAsync(It.IsAny<ValueTuple<Guid, JsonPatchDocument<EditDepartmentRequest>>>(), default))
      .ReturnsAsync(new ValidationResult(new List<ValidationFailure>() { new ValidationFailure() }));

    OperationResultResponse<bool> expectedResponse = new()
    {
      Errors = new List<string> { "Request is not correct." }
    };

    SerializerAssert.AreEqual(expectedResponse, await _command.ExecuteAsync(Guid.NewGuid(), _request));

    Verifiable(
      accessValidatorTimes: Times.Once(),
      editDepartmentRequestValidatorTimes: Times.Once(),
      responseCreatorTimes: Times.Once(),
      departmentRepositoryEditTimes: Times.Never(),
      patchDbDepartmentMapperTimes: Times.Never(),
      departmentUserRepositoryTimes: Times.Never(),
      memoryCacheHelperRemoveTimes: Times.Never(),
      memoryCacheHelperGetDepartmentsTreeTimes: Times.Never(),
      departmentRepositoryRemoveTimes: Times.Never(),
      globalCacheRepositoryTimes: Times.Never(),
      departmentBranchHelperTimes: Times.Never());
  }

  [Test]
  public async Task EditDepartmentWithoutChangeIsActiveSuccessfulyTestAsync()
  {
    OperationResultResponse<bool> expectedResponse = new()
    {
      Body = true
    };

    SerializerAssert.AreEqual(expectedResponse, await _command.ExecuteAsync(Guid.NewGuid(), _request));

    Verifiable(
      accessValidatorTimes: Times.Once(),
      editDepartmentRequestValidatorTimes: Times.Once(),
      patchDbDepartmentMapperTimes: Times.Once(),
      responseCreatorTimes: Times.Never(),
      departmentRepositoryEditTimes: Times.Once(),
      departmentUserRepositoryTimes: Times.Never(),
      memoryCacheHelperRemoveTimes: Times.Once(),
      memoryCacheHelperGetDepartmentsTreeTimes: Times.Never(),
      departmentRepositoryRemoveTimes: Times.Never(),
      globalCacheRepositoryTimes: Times.Once(),
      departmentBranchHelperTimes: Times.Never());
  }

  [Test]
  public async Task EditDepartmentWithChangeIsActiveSuccessfulyTestAsync()
  {
    _request.Operations.Find(x => x.path.EndsWith(nameof(EditDepartmentRequest.IsActive), StringComparison.OrdinalIgnoreCase)).value = false;

    OperationResultResponse<bool> expectedResponse = new()
    {
      Body = true
    };

    SerializerAssert.AreEqual(expectedResponse, await _command.ExecuteAsync(Guid.NewGuid(), _request));

    Verifiable(
      accessValidatorTimes: Times.Once(),
      editDepartmentRequestValidatorTimes: Times.Once(),
      patchDbDepartmentMapperTimes: Times.Once(),
      responseCreatorTimes: Times.Never(),
      departmentRepositoryEditTimes: Times.Once(),
      departmentUserRepositoryTimes: Times.Once(),
      memoryCacheHelperRemoveTimes: Times.Once(),
      memoryCacheHelperGetDepartmentsTreeTimes: Times.Once(),
      departmentRepositoryRemoveTimes: Times.Once(),
      globalCacheRepositoryTimes: Times.Once(),
      departmentBranchHelperTimes: Times.Once());
  }
}
