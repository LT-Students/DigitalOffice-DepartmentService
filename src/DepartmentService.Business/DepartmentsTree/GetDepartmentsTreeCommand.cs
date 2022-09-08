using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.DepartmentsTree.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Constants;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.Extensions.Caching.Memory;

namespace LT.DigitalOffice.DepartmentService.Business.DepartmentsTree
{
  public class GetDepartmentsTreeCommand : IGetDepartmentsTreeCommand
  {
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IDepartmentsTreeInfoMapper _departmentsTreeInfoMapper;
    private readonly IMemoryCache _cache;

    public GetDepartmentsTreeCommand(
      IDepartmentRepository departmentsTreeRepository,
      IDepartmentsTreeInfoMapper departmentsTreeInfoMapper,
      IMemoryCache cache)
    {
      _departmentRepository = departmentsTreeRepository;
      _departmentsTreeInfoMapper = departmentsTreeInfoMapper;
      _cache = cache;
    }

    public async Task<OperationResultResponse<List<DepartmentsTreeInfo>>> ExecuteAsync()
    {
      List<DepartmentsTreeInfo> departmentsTreeCache = _cache.Get<List<DepartmentsTreeInfo>>(CacheKeys.DepartmentsTree);

      if (departmentsTreeCache is null)
      {
        List<Tuple<Guid, string, string, Guid?>> departmentsTree = await _departmentRepository.GetDepartmentsTreeAsync();
        departmentsTreeCache = _cache.Set(CacheKeys.DepartmentsTree, _departmentsTreeInfoMapper.Map(departmentsTree, null));
      }

      return new()
      {
        Body = departmentsTreeCache
      };
    }
  }
}
