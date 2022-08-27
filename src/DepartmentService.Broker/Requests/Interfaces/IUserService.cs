using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.DepartmentUser;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Models;

namespace LT.DigitalOffice.DepartmentService.Broker.Requests.Interfaces
{
  [AutoInject]
  public interface IUserService
  {
    Task<(List<UserData> usersData, int totalCount)> GetFilteredUsersAsync(List<Guid> usersIds, FindDepartmentUsersFilter filter);

    Task<List<Guid>> CheckUsersExistenceAsync(
      List<Guid> usersIds,
      List<string> errors = null);

    Task<List<UserData>> GetUsersDatasAsync(List<Guid> usersIds, List<string> errors);
  }
}
