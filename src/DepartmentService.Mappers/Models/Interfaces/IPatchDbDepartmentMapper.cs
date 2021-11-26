using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.Kernel.Attributes;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces
{
  [AutoInject]
  public interface IPatchDbDepartmentMapper
  {
    JsonPatchDocument<DbDepartment> Map(JsonPatchDocument<EditDepartmentRequest> request);
  }
}
