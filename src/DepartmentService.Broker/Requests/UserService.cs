using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Broker.Requests.Interfaces;
using LT.DigitalOffice.Kernel.BrokerSupport.Helpers;
using LT.DigitalOffice.Kernel.RedisSupport.Constants;
using LT.DigitalOffice.Kernel.RedisSupport.Extensions;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using LT.DigitalOffice.Models.Broker.Common;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Requests.User;
using LT.DigitalOffice.Models.Broker.Responses.User;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.DepartmentService.Broker.Requests
{
  public class UserService : IUserService
  {
    private readonly IRequestClient<IGetUsersDataRequest> _rcGetUsersData;
    private readonly IRequestClient<ICheckUsersExistence> _rcCheckUsersExistence;
    private readonly ILogger<UserService> _logger;
    private readonly IGlobalCacheRepository _globalCache;

    public UserService(
      IRequestClient<IGetUsersDataRequest> rcGetUsersData,
      IRequestClient<ICheckUsersExistence> rcCheckUsersExistence,
      ILogger<UserService> logger,
      IGlobalCacheRepository globalCache)
    {
      _rcGetUsersData = rcGetUsersData;
      _rcCheckUsersExistence = rcCheckUsersExistence;
      _logger = logger;
      _globalCache = globalCache;
    }

    public async Task<List<Guid>> CheckUsersExistenceAsync(
      List<Guid> requestIds,
      List<string> errors = null)
    {
      return (await RequestHandler.ProcessRequest<ICheckUsersExistence, ICheckUsersExistence>(
        _rcCheckUsersExistence,
        ICheckUsersExistence.CreateObj(userIds: requestIds),
        errors,
        _logger))?.UserIds;
    }

    public async Task<List<UserData>> GetUsersDatasAsync(List<Guid> usersIds, List<string> errors)
    {
      if (usersIds is null || !usersIds.Any())
      {
        return null;
      }

      List<UserData> usersData = await _globalCache.GetAsync<List<UserData>>(Cache.Users, usersIds.GetRedisCacheHashCode());

      if (usersData is not null)
      {
        _logger.LogInformation(
          "UsersDatas were taken from the cache. Users ids: {usersIds}", string.Join(", ", usersIds));
      }
      else
      {
        usersData = (await _rcGetUsersData.ProcessRequest<IGetUsersDataRequest, IGetUsersDataResponse>(
          IGetUsersDataRequest.CreateObj(usersIds),
          errors,
          _logger))?.UsersData;
      }

      return usersData;
    }
  }
}
