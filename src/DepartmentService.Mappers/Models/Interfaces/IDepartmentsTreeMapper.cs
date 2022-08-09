using System;
using System.Collections.Generic;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces
{
  [AutoInject]
  public interface IDepartmentsTreeInfoMapper
  {
    List<DepartmentsTreeInfo> Map(List<Tuple<Guid, string, string, Guid?>> result, Guid? idParent);
  }
}
