using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Models.Broker.Models.Image;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models
{
  public class ImageInfoMapper : IImageInfoMapper
  {
    public ImageInfo Map(ImageData imageData)
    {
      return imageData is null
        ? null
        : new ImageInfo
        {
          Id = imageData.ImageId,
          ParentId = imageData.ParentId,
          Content = imageData.Content,
          Extension = imageData.Extension,
          Name = imageData.Name
        };
    }
  }
}
