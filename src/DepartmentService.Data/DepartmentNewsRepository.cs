using System;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Provider;
using LT.DigitalOffice.DepartmentService.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace LT.DigitalOffice.DepartmentService.Data
{
  public class DepartmentNewsRepository : IDepartmentNewsRepository
  {
    private readonly IDataProvider _provider;

    public DepartmentNewsRepository(
      IDataProvider provider)
    {
      _provider = provider;
    }

    public async Task<Guid?> CreateAsync(DbDepartmentNews dbDepartmentNews)
    {
      if (dbDepartmentNews is null)
      {
        return null;
      }

      _provider.DepartmentsNews.Add(dbDepartmentNews);
      await _provider.SaveAsync();

      return dbDepartmentNews.Id;
    }

    public async Task RemoveAsync(Guid newsId, Guid removedBy)
    {
      DbDepartmentNews dbDepartmentNews = await _provider.DepartmentsNews
        .FirstOrDefaultAsync(dn => dn.NewsId == newsId && dn.IsActive);

      if (dbDepartmentNews is not null)
      {
        dbDepartmentNews.IsActive = false;
        dbDepartmentNews.ModifiedAtUtc = DateTime.UtcNow;
        dbDepartmentNews.ModifiedBy = removedBy;

        await _provider.SaveAsync();
      }
    }
  }
}
