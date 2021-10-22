﻿using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Models.Broker.Models;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models
{
  public class ImageInfoMapper : IImageInfoMapper
  {
    public ImageInfo Map(ImageData imageData)
    {
      if (imageData == null)
      {
        return null;
      }

      return new ImageInfo
      {
        Id = imageData.ImageId,
        ParentId = imageData.ParentId,
        Type = imageData.Type,
        Content = imageData.Content,
        Extension = imageData.Extension,
        Name = imageData.Name
      };
    }
  }
}
