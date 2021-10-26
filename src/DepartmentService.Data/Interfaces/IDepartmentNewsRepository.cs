using System;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.DepartmentService.Data.Interfaces
{
  [AutoInject]
  public interface IDepartmentNewsRepository
  {
    Task<Guid?> CreateAsync(DbDepartmentNews dbDepartmentNews);

    Task RemoveAsync(Guid newsId, Guid removedBy);
  }
}
