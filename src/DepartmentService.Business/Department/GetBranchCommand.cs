using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.Department.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Constants;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.Extensions.Caching.Memory;

namespace LT.DigitalOffice.DepartmentService.Business.Department
{
  public class GetBranchCommand : IGetBranchCommand
  {
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IBranchInfoMapper _branchInfoMapper;
    private readonly IMemoryCache _cache;

    public GetBranchCommand(
      IDepartmentRepository departmentRepository,
      IBranchInfoMapper branchInfoMapper,
      IMemoryCache cache
      )
    {
      _departmentRepository = departmentRepository;
      _branchInfoMapper = branchInfoMapper;
      _cache = cache;
    }

    public async Task<OperationResultResponse<List<BranchInfo>>> ExecuteAsync()
    {
      List<BranchInfo> branchesCache = _cache.Get<List<BranchInfo>>(CacheKeys.Branches);

      if (branchesCache is null)
      {
        List<Tuple<Guid, string, Guid?>> branches = await _departmentRepository.GetBranchesAsync();
        branchesCache = _cache.Set(CacheKeys.Branches, _branchInfoMapper.Map(branches, null));
      }

      return new()
      {
        Body = branchesCache
      };
    }
  }
}
