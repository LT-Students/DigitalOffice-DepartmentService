using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Broker.Requests.Interfaces;
using LT.DigitalOffice.Kernel.BrokerSupport.Helpers;
using LT.DigitalOffice.Models.Broker.Common;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.DepartmentService.Broker.Requests
{
  public class UserService : IUserService
  {
    private readonly IRequestClient<ICheckUsersExistence> _rcCheckUsersExistence;
    private readonly ILogger<UserService> _logger;

    public UserService(
      IRequestClient<ICheckUsersExistence> rcCheckUsersExistence,
      ILogger<UserService> logger)
    {
      _rcCheckUsersExistence = rcCheckUsersExistence;
      _logger = logger;
    }

    public async Task<ICheckUsersExistence> CheckUsersExistenceAsync(
      List<Guid> usersIds)
    {
      List<string> errors = new();

      return await RequestHandler.ProcessRequest<ICheckUsersExistence, ICheckUsersExistence>(
        _rcCheckUsersExistence,
        ICheckUsersExistence.CreateObj(userIds: usersIds),
        errors,
        _logger);
    }
  }
}
