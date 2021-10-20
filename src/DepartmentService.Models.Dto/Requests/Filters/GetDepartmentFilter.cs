using System;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Filters
{
  public record GetDepartmentFilter
  {
    [FromQuery(Name = "departmentid")]
    public Guid DepartmentId { get; set; }

    [FromQuery(Name = "includeusers")]
    public bool? IncludeUsers { get; set; } = false;

    [FromQuery(Name = "includeprojects")]
    public bool? IncludeProjects { get; set; } = false;
  }
}
