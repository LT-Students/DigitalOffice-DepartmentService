using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.Department;
using LT.DigitalOffice.DepartmentService.Business.Department.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department;
using LT.DigitalOffice.DepartmentService.Validation.Department.Interfaces;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.UnitTestKernel;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;

namespace LT.DigitalOffice.DepartmentService.Business.UnitTests.Commands.Department
{
  public class CreateDepartmentCommandTests
  {
    private AutoMocker _autoMocker;
    private ICreateDepartmentCommand _command;

    private CreateDepartmentRequest _request;
    private DbDepartment _dbDepartment;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
      _autoMocker = new();

      _autoMocker
        .Setup<IHttpContextAccessor, int>(a => a.HttpContext.Response.StatusCode)
        .Returns(200);

      _autoMocker
        .Setup<IResponseCreator, OperationResultResponse<Guid?>>(
          x => x.CreateFailureResponse<Guid?>(HttpStatusCode.BadRequest, It.IsAny<List<string>>()))
        .Returns(new OperationResultResponse<Guid?>()
        {
          Errors = new() { "Request is not correct." }
        });

      _autoMocker
        .Setup<IResponseCreator, OperationResultResponse<Guid?>>(
          x => x.CreateFailureResponse<Guid?>(HttpStatusCode.Forbidden, It.IsAny<List<string>>()))
        .Returns(new OperationResultResponse<Guid?>()
        {
          Errors = new() { "Not enough rights." }
        });

      _command = _autoMocker.CreateInstance<CreateDepartmentCommand>();

      _request = new()
      {
        Name = "Name",
        Description = "Description",
        Users = new()
      };

      _dbDepartment = new()
      {
        Id = Guid.NewGuid(),
        Name = "Name",
        Description = "Description",
        IsActive = true,
        CreatedBy = Guid.NewGuid(),
        CreatedAtUtc = DateTime.UtcNow
      };
    }

    [SetUp]
    public void Setup()
    {
      _autoMocker.GetMock<IAccessValidator>().Reset();
      _autoMocker.GetMock<ICreateDepartmentRequestValidator>().Reset();
      _autoMocker.GetMock<IDbDepartmentMapper>().Reset();
      _autoMocker.GetMock<IDepartmentRepository>().Reset();

      _autoMocker
        .Setup<IAccessValidator, Task<bool>>(x => x.HasRightsAsync(Rights.AddEditRemoveDepartments))
        .ReturnsAsync(true);

      _autoMocker
        .Setup<ICreateDepartmentRequestValidator, bool>(x => x.ValidateAsync(_request, default).Result.IsValid)
        .Returns(true);

      _autoMocker
        .Setup<IDbDepartmentMapper, DbDepartment>(x => x.Map(_request))
        .Returns(_dbDepartment);

      _autoMocker
        .Setup<IDepartmentRepository, Task<Guid?>>(x => x.CreateAsync(_dbDepartment))
        .ReturnsAsync(_dbDepartment.Id);
    }

    [Test]
    public async Task ShouldReturnFailedResponseWhenUserHasNotRightAsync()
    {
      _autoMocker
       .Setup<IAccessValidator, Task<bool>>(x => x.HasRightsAsync(Rights.AddEditRemoveDepartments))
       .ReturnsAsync(false);

      OperationResultResponse<Guid?> expectedResponse = new()
      {
        Errors = new List<string> { "Not enough rights." }
      };

      SerializerAssert.AreEqual(expectedResponse, await _command.ExecuteAsync(_request));

      _autoMocker.Verify<IAccessValidator>(
        x => x.HasRightsAsync(It.IsAny<int>()),
        Times.Once);

      _autoMocker.Verify<ICreateDepartmentRequestValidator>(
        x => x.ValidateAsync(It.IsAny<CreateDepartmentRequest>(), It.IsAny<CancellationToken>()),
        Times.Never);

      _autoMocker.Verify<IDbDepartmentMapper>(
        x => x.Map(It.IsAny<CreateDepartmentRequest>()),
        Times.Never);

      _autoMocker.Verify<IDepartmentRepository>(
        x => x.CreateAsync(It.IsAny<DbDepartment>()),
        Times.Never);
    }

    [Test]
    public async Task ShouldReturnFailedResponseWhenValidationIsFailedAsync()
    {
      _autoMocker
        .Setup<ICreateDepartmentRequestValidator, bool>(x => x.ValidateAsync(_request, default).Result.IsValid)
        .Returns(false);

      OperationResultResponse<Guid?> expectedResponse = new()
      {
        Errors = new List<string> { "Request is not correct." }
      };

      SerializerAssert.AreEqual(expectedResponse, await _command.ExecuteAsync(_request));

      _autoMocker.Verify<IAccessValidator>(
        x => x.HasRightsAsync(It.IsAny<int>()),
        Times.Once);

      _autoMocker.Verify<ICreateDepartmentRequestValidator>(
        x => x.ValidateAsync(It.IsAny<CreateDepartmentRequest>(), It.IsAny<CancellationToken>()),
        Times.Once);

      _autoMocker.Verify<IDbDepartmentMapper>(
        x => x.Map(It.IsAny<CreateDepartmentRequest>()),
        Times.Never);

      _autoMocker.Verify<IDepartmentRepository>(
        x => x.CreateAsync(It.IsAny<DbDepartment>()),
        Times.Never);
    }

    [Test]
    public async Task ShouldReturnFailedResponseWhenRepositoryReturnNullAsync()
    {
      _autoMocker
        .Setup<IDepartmentRepository, Task<Guid?>>(x => x.CreateAsync(_dbDepartment))
        .ReturnsAsync((Guid?)null);

      OperationResultResponse<Guid?> expectedResponse = new()
      {
        Errors = new List<string> { "Request is not correct." }
      };

      SerializerAssert.AreEqual(expectedResponse, await _command.ExecuteAsync(_request));

      _autoMocker.Verify<IAccessValidator>(
        x => x.HasRightsAsync(It.IsAny<int>()),
        Times.Once);

      _autoMocker.Verify<ICreateDepartmentRequestValidator>(
        x => x.ValidateAsync(It.IsAny<CreateDepartmentRequest>(), It.IsAny<CancellationToken>()),
        Times.Once);

      _autoMocker.Verify<IDbDepartmentMapper>(
        x => x.Map(It.IsAny<CreateDepartmentRequest>()),
        Times.Once);

      _autoMocker.Verify<IDepartmentRepository>(
        x => x.CreateAsync(It.IsAny<DbDepartment>()),
        Times.Once);
    }

    [Test]
    public async Task ShouldCreateDepartmentSuccesfullAsync()
    {
      OperationResultResponse<Guid?> expectedResponse = new()
      {
        Body = _dbDepartment.Id
      };

      SerializerAssert.AreEqual(expectedResponse, await _command.ExecuteAsync(_request));

      _autoMocker.Verify<IAccessValidator>(
        x => x.HasRightsAsync(It.IsAny<int>()),
        Times.Once);

      _autoMocker.Verify<ICreateDepartmentRequestValidator>(
        x => x.ValidateAsync(It.IsAny<CreateDepartmentRequest>(), It.IsAny<CancellationToken>()),
        Times.Once);

      _autoMocker.Verify<IDbDepartmentMapper>(
        x => x.Map(It.IsAny<CreateDepartmentRequest>()),
        Times.Once);

      _autoMocker.Verify<IDepartmentRepository>(
        x => x.CreateAsync(It.IsAny<DbDepartment>()),
        Times.Once);
    }
  }
}
