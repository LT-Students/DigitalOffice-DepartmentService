using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models
{
  public class DepartmentInfoMapper : IDepartmentInfoMapper
  {
    private readonly ICategoryInfoMapper _categoryInfoMapper;

    public DepartmentInfoMapper(
      ICategoryInfoMapper categoryInfoMapper)
    {
      _categoryInfoMapper = categoryInfoMapper;
    }

    public DepartmentInfo Map(DbDepartment dbDepartment, DepartmentUserInfo director)
    {
      return dbDepartment is null
        ? null
        : new DepartmentInfo
        {
          Id = dbDepartment.Id,
          Name = dbDepartment.Name,
          ShortName = dbDepartment.ShortName,
          Description = dbDepartment.Description,
          Director = director,
          IsActive = dbDepartment.IsActive,
          ParentId = dbDepartment.ParentId,
          Category = _categoryInfoMapper.Map(dbDepartment.Category),
          CountUsers = dbDepartment.Users.Count
        };
    }
  }
}
