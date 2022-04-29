using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Common;

namespace LT.DigitalOffice.DepartmentService.Broker.Requests.Interfaces
{
  [AutoInject]
  public interface IUserService
  {
    Task<ICheckUsersExistence> CheckUsersExistenceAsync(
      List<Guid> usersIds);
  }
}
