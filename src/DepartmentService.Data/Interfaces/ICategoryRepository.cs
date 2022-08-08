using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Category.Filters;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.DepartmentService.Data.Interfaces
{
  [AutoInject]
  public interface ICategoryRepository
  {
    Task<Guid?> CreateAsync(DbCategory category);

    Task<bool> RemoveAsync(Guid categoryId);

    Task<(List<DbCategory> dbCategories, int totalCount)> FindCategoriesAsync(FindCategoriesFilter filter);

    Task<bool> DoesCategoryAlreadyExistAsync(string categoryName);
  }
}
