using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Broker.Helpers.Branch.Interfaces;
using LT.DigitalOffice.DepartmentService.Broker.Helpers.MemoryCache.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.Models.Broker.Enums;

namespace LT.DigitalOffice.DepartmentService.Broker.Helpers.Branch
{
  public class DepartmentBranchHelper : IDepartmentBranchHelper
  {
    private readonly IDepartmentUserRepository _departmentUserRepository;
    private readonly IMemoryCacheHelper _memoryCacheHelper;

    public DepartmentBranchHelper(
      IDepartmentUserRepository userRepository,
      IMemoryCacheHelper memoryCacheHelper)
    {
      _departmentUserRepository = userRepository;
      _memoryCacheHelper = memoryCacheHelper;
    }

    public List<Guid> GetChildrenIds(in List<Tuple<Guid, string, string, Guid?>> listDepartments, Guid parentDepartmentId)
    {
      if (listDepartments is null)
      {
        return Enumerable.Empty<Guid>().ToList();
      }

      List<Guid> childrenIds = new();
      Stack<Guid> childrenIdsStack = new();

      Dictionary<Guid, IEnumerable<Guid>> childrenDictionary = listDepartments.GroupBy(ld => ld.Item4).Where(ld => ld.Key is not null)
        .ToDictionary(ld => ld.Key.Value, ld => ld.Select(e => e.Item1));

      childrenIdsStack.Push(parentDepartmentId);

      while (childrenIdsStack.Any())
      {
        Guid parentId = childrenIdsStack.Pop();

        if (childrenDictionary.TryGetValue(parentId, out IEnumerable<Guid> currentChildrenIds))
        {
          childrenIds.AddRange(currentChildrenIds);

          foreach (var childId in currentChildrenIds)
          {
            childrenIdsStack.Push(childId);
          }
        }
      }

      return childrenIds;
    }

    public async Task<DepartmentUserRole?> GetDepartmentUserRole(Guid userId, Guid departmentId)
    {
      DbDepartmentUser dbUser = await _departmentUserRepository.GetAsync(userId);

      if (dbUser is null || dbUser.DepartmentId == departmentId)
      {
        return (DepartmentUserRole?)dbUser?.Role;
      }

      return GetChildrenIds(await _memoryCacheHelper.GetDepartmentsTreeAsync(), dbUser.DepartmentId)?.Contains(departmentId) == true
        ? (DepartmentUserRole?)dbUser.Role
        : null;
    }
  }
}
