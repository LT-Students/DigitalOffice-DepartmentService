using System;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Filters
{
  public record GetDepartmentFilter
  {
    [FromQuery(Name = "departmentid")]
    public Guid DepartmentId { get; set; }

    [FromQuery(Name = "includeusers")]
    public bool? IncludeUsers { get; set; }

    [FromQuery(Name = "includeprojects")]
    public bool? IncludeProjects { get; set; }

    public bool IsIncludeUsers => IncludeUsers.HasValue && IncludeUsers.Value;
    public bool IsIncludeProjects => IncludeProjects.HasValue && IncludeProjects.Value;
  }
}
