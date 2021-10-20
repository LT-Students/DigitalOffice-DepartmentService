using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;

namespace LT.DigitalOffice.DepartmentService.Business.User.Interfaces
{
  [AutoInject]
  public interface IRemoveDepartmentUserRequest
  {
    Task<OperationResultResponse<bool>> ExecuteAsync(Guid departmentId, List<Guid> userIds);
  }
}
