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
    Task CreateAsync(DbCategory category);

    Task<(List<DbCategory> dbCategories, int totalCount)> FindAsync(FindCategoriesFilter filter);

    Task<bool> ExistAsync(string categoryName);

    Task<bool> IdExistAsync(Guid categoryId);
  }
}
