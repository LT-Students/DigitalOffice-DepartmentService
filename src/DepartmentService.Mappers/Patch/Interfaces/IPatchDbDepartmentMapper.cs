using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department;
using LT.DigitalOffice.Kernel.Attributes;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.DepartmentService.Mappers.Patch.Interfaces
{
  [AutoInject]
  public interface IPatchDbDepartmentMapper
  {
    JsonPatchDocument<DbDepartment> Map(JsonPatchDocument<EditDepartmentRequest> request);
  }
}
