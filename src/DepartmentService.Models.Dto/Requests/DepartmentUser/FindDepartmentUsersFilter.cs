using System;
using LT.DigitalOffice.Kernel.Requests;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Requests.DepartmentUser
{
  public record FindDepartmentUsersFilter : BaseFindFilter
  {
    [FromQuery(Name = "isActive")]
    public bool? IsActive { get; set; }

    [FromQuery(Name = "isAscendingSort")]
    public bool? IsAscendingSort { get; set; } = null;

    [FromQuery(Name = "fullnameincludesubstring")]
    public string FullNameIncludeSubstring { get; set; }

    [FromQuery(Name = "departmentUserRoleAscendingSort")]
    public bool? DepartmentUserRoleAscendingSort { get; set; } = null;

    [FromQuery(Name = "includePositions")]
    public bool IncludePositions { get; set; } = false;

    [FromQuery(Name = "byPositionId")]
    public Guid? byPositionId { get; set; } = null;
  }
}
