using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Provider;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Category.Filters;
using Microsoft.EntityFrameworkCore;

namespace LT.DigitalOffice.DepartmentService.Data
{
  public class CategoryRepository : ICategoryRepository
  {
    private readonly IDataProvider _provider;

    public CategoryRepository(IDataProvider provider)
    {
      _provider = provider;
    }

    public Task CreateAsync(DbCategory category)
    {
      _provider.Categories.Add(category);
      
      return _provider.SaveAsync();
    }

    public async Task<(List<DbCategory> dbCategories, int totalCount)> FindAsync(FindCategoriesFilter filter)
    {
      if (filter is null)
      {
        return (null, default);
      }

      IQueryable<DbCategory> dbCategories = _provider.Categories.AsQueryable();

      if (!string.IsNullOrWhiteSpace(filter.NameIncludeSubstring))
      {
        dbCategories = dbCategories.Where(g => g.Name.ToLower().Contains(filter.NameIncludeSubstring.ToLower()));
      }

      if (filter.IsAscendingSort.HasValue)
      {
        dbCategories = filter.IsAscendingSort.Value
          ? dbCategories.OrderBy(d => d.Name)
          : dbCategories.OrderByDescending(d => d.Name);
      }

      return (
        await dbCategories
          .Skip(filter.SkipCount)
          .Take(filter.TakeCount)
          .ToListAsync(),
        await dbCategories.CountAsync());
    }

    public Task<bool> ExistAsync(string categoryName)
    {
      return _provider.Categories.AnyAsync(s => s.Name.ToLower() == categoryName.ToLower());
    }

    public Task<bool> IdExistAsync(Guid categoryId)
    {
      return _provider.Categories.AnyAsync(x => x.Id == categoryId);
    }
  }
}
