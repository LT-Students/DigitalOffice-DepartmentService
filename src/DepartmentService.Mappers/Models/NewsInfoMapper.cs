using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Models.Broker.Models.News;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models
{
  public class NewsInfoMapper : INewsInfoMapper
  {
    public NewsInfo Map(NewsData newsData, UserInfo author, UserInfo sender)
    {
      if (newsData == null)
      {
        return null;
      }

      return new NewsInfo
      {
        Id = newsData.Id,
        Preview = newsData.Preview,
        Subject = newsData.Subject,
        Pseudonym = newsData.Pseudonym,             
        Author = author,
        CreatedAtUtc =  newsData.CreatedAtUtc,
        Sender = sender 
      };
    }
  }
}
