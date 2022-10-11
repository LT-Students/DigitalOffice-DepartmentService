using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.DepartmentService.Broker.Helpers.MemoryCache.Interfaces
{
  [AutoInject]
  public interface IMemoryCacheHelper
  {
    Task<List<DepartmentsTreeInfo>> GetDepartmentsTreeInfoAsync();
    Task<List<Tuple<Guid, string, string, Guid?>>> GetDepartmentsTreeAsync();
    void Remove(params string[] keys);
  }
}
