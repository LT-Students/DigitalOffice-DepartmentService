using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.Department;
using LT.DigitalOffice.DepartmentService.Business.Department.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Responses.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department.Filters;
using LT.DigitalOffice.DepartmentService.Models.Dto.Responses;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.UnitTestKernel;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;

namespace LT.DigitalOffice.DepartmentService.Business.UnitTests.Commands.Department;

public class GetDepartmentCommandTests
{
  private AutoMocker _autoMocker;
  private IGetDepartmentCommand _command;

  private DbDepartment _dbDepartment;
  private DepartmentResponse _departmentResponse;

  private DbDepartmentUser _dbDepartmentUser;
  private DepartmentUserInfo _departmentUserInfo;

  private DbCategory _dbCategory;
  private CategoryInfo _categoryInfo;

  private void Verifiable(
    Times responseCreatorTimes,
    Times projectRepositoryTimes,
    Times departmentResponseMapperTimes)
  {
    _autoMocker.Verify<IResponseCreator, OperationResultResponse<DepartmentResponse>>(
      x => x.CreateFailureResponse<DepartmentResponse>(It.IsAny<HttpStatusCode>(), It.IsAny<List<string>>()), responseCreatorTimes);

    _autoMocker.Verify<IDepartmentRepository, Task<DbDepartment>>(
      x => x.GetAsync(It.IsAny<GetDepartmentFilter>(), It.IsAny<CancellationToken>()), projectRepositoryTimes);

    _autoMocker.Verify<IDepartmentResponseMapper, DepartmentResponse>(
      x => x.Map(It.IsAny<DbDepartment>()), departmentResponseMapperTimes);
  }

  [OneTimeSetUp]
  public void OneTimeSetUp()
  {
    _autoMocker = new();
    _command = _autoMocker.CreateInstance<GetDepartmentCommand>();

    _dbCategory = new()
    {
      Id = Guid.NewGuid(),
      Name = "Name"
    };

    _dbDepartmentUser = new()
    {
      UserId = Guid.NewGuid(),
      Role = 0,
      Assignment = 0
    };

    _dbDepartment = new()
    {
      Id = Guid.NewGuid(),
      ParentId = Guid.NewGuid(),
      Name = "Name",
      ShortName = "ShortName",
      Description = "Description",
      IsActive = true,
      Category = _dbCategory,
      Users = new List<DbDepartmentUser>
      {
        _dbDepartmentUser
      }
    };

    _categoryInfo = new()
    {
      Id = _dbCategory.Id,
      Name = _dbCategory.Name
    };

    _departmentUserInfo = new()
    {
      UserId = _dbDepartmentUser.UserId,
      Role = (DepartmentUserRole)_dbDepartmentUser.Role,
      Assignment = (DepartmentUserAssignment)_dbDepartmentUser.Assignment
    };

    _departmentResponse = new()
    {
      Id = _dbDepartment.Id,
      ParentId = _dbDepartment.ParentId,
      Name = _dbDepartment.Name,
      ShortName = _dbDepartment.ShortName,
      Description = _dbDepartment.Description,
      IsActive = _dbDepartment.IsActive,
      Category = _categoryInfo,
      Users = new List<DepartmentUserInfo>
      {
        _departmentUserInfo
      }
    };
  }

  [SetUp]
  public void SetUp()
  {
    _autoMocker.GetMock<IDepartmentRepository>().Reset();
    _autoMocker.GetMock<IDepartmentResponseMapper>().Reset();
  }

  [Test]
  public async Task ShouldReturnDbDepartment()
  {
    _autoMocker
      .Setup<IDepartmentRepository, Task<DbDepartment>>(x => x.GetAsync(It.IsAny<GetDepartmentFilter>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(_dbDepartment);

    _autoMocker
      .Setup<IDepartmentResponseMapper, DepartmentResponse>(x => x.Map(It.IsAny<DbDepartment>()))
      .Returns(_departmentResponse);

    OperationResultResponse<DepartmentResponse> expectedResponse = new()
    {
      Body = _departmentResponse
    };

    SerializerAssert.AreEqual(expectedResponse, await _command.ExecuteAsync(It.IsAny<GetDepartmentFilter>(), It.IsAny<CancellationToken>()));

    Verifiable(
      projectRepositoryTimes: Times.Once(),
      responseCreatorTimes: Times.Never(),
      departmentResponseMapperTimes: Times.Once());
  }

  [Test]
  public async Task ShouldReturnNotFound()
  {
    _autoMocker
      .Setup<IDepartmentRepository, Task<DbDepartment>>(x => x.GetAsync(It.IsAny<GetDepartmentFilter>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(It.IsAny<DbDepartment>);

    OperationResultResponse<DepartmentResponse> expectedResponse = new()
    {
      Errors = new List<string> { "Not found." }
    };

    _autoMocker
      .Setup<IResponseCreator, OperationResultResponse<DepartmentResponse>>(x => x.CreateFailureResponse<DepartmentResponse>(HttpStatusCode.NotFound, default))
      .Returns(expectedResponse);

    SerializerAssert.AreEqual(expectedResponse, await _command.ExecuteAsync(It.IsAny<GetDepartmentFilter>()));

    Verifiable(
      responseCreatorTimes: Times.Once(),
      projectRepositoryTimes: Times.Once(),
      departmentResponseMapperTimes: Times.Never());
  }
}
