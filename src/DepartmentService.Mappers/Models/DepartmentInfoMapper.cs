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

    public DepartmentInfo Map(DbDepartment dbDepartment, UserInfo director)
    {
      return dbDepartment is null
        ? null
        : new DepartmentInfo
        {
          Id = dbDepartment.Id,
          Name = dbDepartment.Name,
          ShortName = dbDepartment.ShortName,
          IsActive = dbDepartment.IsActive,
          ParentId = dbDepartment.ParentId,
          CountUsers = dbDepartment.Users.Count,
          Category = _categoryInfoMapper.Map(dbDepartment.Category),
          Director = director
        };
    }
  }
}
