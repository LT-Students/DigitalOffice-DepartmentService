using LT.DigitalOffice.Kernel.Requests;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Filters
{
  public record FindDepartmentFilter : BaseFindFilter
  {
    [FromQuery(Name = "isAscendingSort")]
    public bool? IsAscendingSort { get; set; }

    [FromQuery(Name = "isActive")]
    public bool? IsActive { get; set; }
  }
}
