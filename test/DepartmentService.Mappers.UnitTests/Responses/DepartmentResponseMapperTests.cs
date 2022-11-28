using System;
using System.Collections.Generic;
using LT.DigitalOffice.CompanyService.Mappers.Responses;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Responses.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.DepartmentService.Models.Dto.Responses;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.UnitTestKernel;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;

namespace LT.DigitalOffice.DepartmentService.Mappers.UnitTests.Responses;

public class DepartmentResponseMapperTests
{
  private IDepartmentResponseMapper _mapper;
  private AutoMocker _mocker;

  private DbDepartment _dbDepartment;
  private DepartmentResponse _response;

  private DbDepartmentUser _dbDepartmentUser;
  private DepartmentUserInfo _departmentUserInfo;

  private DbCategory _dbCategory;
  private CategoryInfo _categoryInfo;

  private void Verifiable(
    Times categoryMapperTimes,
    Times departmentUserMapperTimes)
  {
    _mocker.Verify<ICategoryInfoMapper, CategoryInfo>(
      x => x.Map(It.IsAny<DbCategory>()), categoryMapperTimes);

    _mocker.Verify<IDepartmentUserInfoMapper, DepartmentUserInfo>(
      x => x.Map(It.IsAny<DbDepartmentUser>()), departmentUserMapperTimes);
  }

  private void CreateModels()
  {
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

    _response = new()
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
    CreateModels();

    _mocker = new();
    _mapper = _mocker.CreateInstance<DepartmentResponseMapper>();

    _mocker
      .Setup<ICategoryInfoMapper, CategoryInfo>(x => x.Map(It.IsAny<DbCategory>()))
      .Returns(_categoryInfo);

    _mocker
      .Setup<IDepartmentUserInfoMapper, DepartmentUserInfo>(x => x.Map(It.IsAny<DbDepartmentUser>()))
      .Returns(_departmentUserInfo);
  }

  [Test]
  public void ShouldReturnNullWhenRequestMappingObjectIsNull()
  {
    Assert.AreEqual(null, _mapper.Map(null));

    Verifiable(
      categoryMapperTimes: Times.Never(),
      departmentUserMapperTimes: Times.Never());
  }

  [Test]
  public void ShouldReturnDepartmentResponseWhenMappingDbDepartment()
  {
    SerializerAssert.AreEqual(_response, _mapper.Map(_dbDepartment));

    Verifiable(
      categoryMapperTimes: Times.Once(),
      departmentUserMapperTimes: Times.Once());
  }
}

