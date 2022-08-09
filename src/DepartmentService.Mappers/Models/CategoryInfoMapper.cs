using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models
{
  public class CategoryInfoMapper : ICategoryInfoMapper
  {
    public CategoryInfo Map(DbCategory dbCategory)
    {
      return dbCategory is null
        ? default
        : new CategoryInfo
        {
          Id = dbCategory.Id,
          Name = dbCategory.Name
        };
    }
  }
}
