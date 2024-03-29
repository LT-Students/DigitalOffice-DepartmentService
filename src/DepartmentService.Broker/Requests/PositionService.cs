﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Broker.Requests.Interfaces;
using LT.DigitalOffice.Kernel.BrokerSupport.Helpers;
using LT.DigitalOffice.Kernel.RedisSupport.Constants;
using LT.DigitalOffice.Kernel.RedisSupport.Extensions;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using LT.DigitalOffice.Models.Broker.Models.Position;
using LT.DigitalOffice.Models.Broker.Requests.Position;
using LT.DigitalOffice.Models.Broker.Responses.Position;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.DepartmentService.Broker.Requests
{
  public class PositionService : IPositionService
  {
    private readonly IRequestClient<IGetPositionsRequest> _rcGetPositionsData;
    private readonly IRequestClient<IFilterPositionsRequest> _rcGetPosotions;
    private readonly ILogger<PositionService> _logger;
    private readonly IGlobalCacheRepository _globalCache;

    public PositionService(
      IRequestClient<IGetPositionsRequest> rcGetPositionsData,
      IRequestClient<IFilterPositionsRequest> rcGetPosotions,
      ILogger<PositionService> logger,
      IGlobalCacheRepository globalCache)
    {
      _rcGetPositionsData = rcGetPositionsData;
      _rcGetPosotions = rcGetPosotions;
      _logger = logger;
      _globalCache = globalCache;
    }

    public async Task<List<PositionData>> GetPositionsAsync(
      List<Guid> usersIds,
      List<string> errors = null,
      CancellationToken cancellationToken = default)
    {
      if (usersIds is null)
      {
        return null;
      }

      object request = IGetPositionsRequest.CreateObj(usersIds);

      List<PositionData> positions = await _globalCache.GetAsync<List<PositionData>>(Cache.Positions, usersIds.GetRedisCacheKey(
        nameof(IGetPositionsRequest), request.GetBasicProperties()));

      if (positions is not null)
      {
        _logger.LogInformation($"Positions were taken from the cache.");
      }
      else
      {
        positions = (await RequestHandler.ProcessRequest<IGetPositionsRequest, IGetPositionsResponse>(
            _rcGetPositionsData,
            request,
            errors,
            _logger))
          ?.Positions;
      }

      return positions;
    }

    public async Task<List<PositionFilteredData>> GetPositionFilteredDataAsync(List<Guid> positionsIds, List<string> errors)
    {
      if (positionsIds is null || !positionsIds.Any())
      {
        return null;
      }

      object request = IFilterPositionsRequest.CreateObj(positionsIds);

      List<PositionFilteredData> positionsData = await _globalCache.GetAsync<List<PositionFilteredData>>(Cache.Positions, positionsIds.GetRedisCacheKey(
        nameof(IFilterPositionsRequest), request.GetBasicProperties()));

      if (positionsData is null)
      {
        positionsData =
          (await RequestHandler.ProcessRequest<IFilterPositionsRequest, IFilterPositionsResponse>(
            _rcGetPosotions,
            request,
            errors,
            _logger))
          ?.Positions;
      }
      if (positionsData is null)
      {
        errors.Add("Can not filter by positions.");
      }

      return positionsData;
    }
  }
}
