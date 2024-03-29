﻿using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Responses;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.DepartmentService.Mappers.Responses.Interfaces
{
  [AutoInject]
  public interface IDepartmentResponseMapper
  {
    DepartmentResponse Map(DbDepartment dbDepartment);
  }
}
