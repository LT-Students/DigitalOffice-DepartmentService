using System.Linq;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Responses.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Responses;

namespace LT.DigitalOffice.CompanyService.Mappers.Responses
{
  public class DepartmentResponseMapper : IDepartmentResponseMapper
  {
    private readonly ICategoryInfoMapper _categoryMapper;
    private readonly IDepartmentUserInfoMapper _userMapper;

    public DepartmentResponseMapper(
      ICategoryInfoMapper categoryMapper,
      IDepartmentUserInfoMapper userMapper)
    {
      _categoryMapper = categoryMapper;
      _userMapper = userMapper;
    }

    public DepartmentResponse Map(DbDepartment dbDepartment)
    {
      return dbDepartment is null
        ? null
        : new DepartmentResponse
        {
          Id = dbDepartment.Id,
          Name = dbDepartment.Name,
          ShortName = dbDepartment.ShortName,
          Description = dbDepartment.Description,
          IsActive = dbDepartment.IsActive,
          ParentId = dbDepartment.ParentId,
          Category = _categoryMapper.Map(dbDepartment.Category),
          Users = dbDepartment.Users?.Select(_userMapper.Map)
        };
    }
  }
}
