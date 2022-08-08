using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    public async Task<Guid?> CreateAsync(DbCategory category)
    {
      _provider.Categories.Add(category);
      await _provider.SaveAsync();

      return category.Id;
    }

    public async Task<bool> RemoveAsync(Guid categoryId)
    {
      DbCategory category = await _provider.Categories.Where(t => t.Id == categoryId).FirstOrDefaultAsync();
      
      _provider.Categories.Remove(category);
      await _provider.SaveAsync();

      return true;
    }

    public async Task<(List<DbCategory> dbCategories, int totalCount)> FindCategoriesAsync(FindCategoriesFilter filter)
    {
      if (filter is null)
      {
        return (null, default);
      }

      IQueryable<DbCategory> query = _provider.Categories.AsQueryable();

      if (!string.IsNullOrWhiteSpace(filter.NameIncludeSubstring))
      {
        query = query.Where(g => g.Name.ToLower().Contains(filter.NameIncludeSubstring.ToLower()));
      }

      return (
        await query.Skip(filter.SkipCount).Take(filter.TakeCount).ToListAsync(),
        await query.CountAsync());
    }

    public Task<bool> DoesCategoryAlreadyExistAsync(string categoryName)
    {
      return _provider.Categories.AnyAsync(s => s.Name.ToLower() == categoryName.ToLower());
    }
  }
}
