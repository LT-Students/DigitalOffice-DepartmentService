﻿using System;
using System.Collections.Generic;
using LT.DigitalOffice.Models.Broker.Enums;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Requests.DepartmentUser
{
  public record EditDepartmentUsersRoleRequest
  {
    public DepartmentUserRole Role { get; set; }
    public List<Guid> UsersIds { get; set; }    
  }
}
