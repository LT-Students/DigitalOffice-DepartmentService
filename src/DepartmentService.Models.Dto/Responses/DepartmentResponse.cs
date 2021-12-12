using System.Collections.Generic;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Responses
{
  public record DepartmentResponse
  {
    public DepartmentInfo Department { get; set; }
    public IEnumerable<UserInfo> Users { get; set; }
    public IEnumerable<ProjectInfo> Projects { get; set; }
    public IEnumerable<NewsInfo> News { get; set; }
  }
}
