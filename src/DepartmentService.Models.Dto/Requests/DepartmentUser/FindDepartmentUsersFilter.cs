using LT.DigitalOffice.Kernel.Requests;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Requests.DepartmentUser
{
  public record FindDepartmentUsersFilter : BaseFindFilter
  {
    [FromQuery(Name = "isActive")]
    public bool? IsActive { get; set; }

    [FromQuery(Name = "ascendingSort")]
    public bool? AscendingSort { get; set; } = null;

    [FromQuery(Name = "departmentUserRoleAscendingSort")]
    public bool? DepartmentUserRoleAscendingSort { get; set; } = null;

    [FromQuery(Name = "includeAvatars")]
    public bool IncludeAvatars { get; set; } = false;

    [FromQuery(Name = "includePositions")]
    public bool IncludePositions { get; set; } = false;
  }
}
