using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Broker.Helpers.MemoryCache.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Constants;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using Microsoft.Extensions.Caching.Memory;

namespace LT.DigitalOffice.DepartmentService.Broker.Helpers.MemoryCache
{
  public class MemoryCacheHelper : IMemoryCacheHelper
  {
    private readonly IMemoryCache _memoryCache;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IDepartmentsTreeInfoMapper _departmentsTreeInfoMapper;

    public MemoryCacheHelper(
      IMemoryCache memoryCache,
      IDepartmentRepository departmentRepository,
      IDepartmentsTreeInfoMapper departmentsTreeInfoMapper)
    {
      _memoryCache = memoryCache;
      _departmentRepository = departmentRepository;
      _departmentsTreeInfoMapper = departmentsTreeInfoMapper;
    }

    public async Task<List<DepartmentsTreeInfo>> GetDepartmentsTreeInfoAsync()
    {
      List<DepartmentsTreeInfo> departmentsTreeInfoCache = _memoryCache.Get<List<DepartmentsTreeInfo>>(CacheKeys.DepartmentsTreeInfo);

      if (departmentsTreeInfoCache is null)
      {
        List<Tuple<Guid, string, string, Guid?>> departmentsTree = await GetDepartmentsTreeAsync();

        departmentsTreeInfoCache = _memoryCache.Set(CacheKeys.DepartmentsTreeInfo, _departmentsTreeInfoMapper.Map(departmentsTree, null));
      }

      return departmentsTreeInfoCache;
    }

    public async Task<List<Tuple<Guid, string, string, Guid?>>> GetDepartmentsTreeAsync()
    {
      List<Tuple<Guid, string, string, Guid?>> departmentsTreeCache = _memoryCache.Get<List<Tuple<Guid, string, string, Guid?>>>(CacheKeys.DepartmentsTree);

      if (departmentsTreeCache is null)
      {
        List<Tuple<Guid, string, string, Guid?>> departmentsTree = await _departmentRepository.GetDepartmentsTreeAsync();

        departmentsTreeCache = _memoryCache.Set(CacheKeys.DepartmentsTree, departmentsTree);
      }

      return departmentsTreeCache;
    }

    public void Remove(params string[] keys)
    {
      if (keys is not null)
      {
        foreach (var key in keys)
        {
          _memoryCache.Remove(key);
        }
      }
    }
  }
}
