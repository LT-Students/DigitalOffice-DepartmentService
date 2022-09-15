using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Broker.Requests.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Kernel.BrokerSupport.Helpers;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Requests.Image;
using LT.DigitalOffice.Models.Broker.Responses.Image;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.DepartmentService.Broker.Requests
{
  public class ImageService : IImageService
  {
    private readonly IRequestClient<IGetImagesRequest> _rcGetImages;
    private readonly IImageInfoMapper _mapper;
    private readonly ILogger<ImageService> _logger;

    public ImageService(
      IRequestClient<IGetImagesRequest> rcGetImages,
      IImageInfoMapper mapper,
      ILogger<ImageService> logger)
    {
      _rcGetImages = rcGetImages;
      _mapper = mapper;
      _logger = logger;
    }

    public async Task<List<ImageInfo>> GetImagesAsync(
      List<Guid> imagesIds,
      ImageSource imageSourse,
      List<string> errors,
      CancellationToken cancellationToken = default)
    {
      return imagesIds is null || !imagesIds.Any()
        ? null
        : (await _rcGetImages.ProcessRequest<IGetImagesRequest, IGetImagesResponse>(
            IGetImagesRequest.CreateObj(imagesIds, imageSourse),
            errors,
            _logger))?.ImagesData.Select(_mapper.Map).ToList();
    }
  }
}
