using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Broker.Helpers.MemoryCache.Interfaces;
using LT.DigitalOffice.DepartmentService.Business.DepartmentsTree.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Kernel.Responses;

namespace LT.DigitalOffice.DepartmentService.Business.DepartmentsTree
{
  public class GetDepartmentsTreeCommand : IGetDepartmentsTreeCommand
  {
    private readonly IMemoryCacheHelper _memoryCacheHelper;

    public GetDepartmentsTreeCommand(
      IMemoryCacheHelper memoryCacheHelper)
    {
      _memoryCacheHelper = memoryCacheHelper;
    }

    public async Task<OperationResultResponse<List<DepartmentsTreeInfo>>> ExecuteAsync()
    {
      return new()
      {
        Body = await _memoryCacheHelper.GetDepartmentsTreeInfoAsync()
      };
    }
  }
}
