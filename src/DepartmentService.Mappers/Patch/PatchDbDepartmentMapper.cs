using System.Linq;
using LT.DigitalOffice.DepartmentService.Mappers.Patch.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace LT.DigitalOffice.DepartmentService.Mappers.Patch
{
  public class PatchDbDepartmentMapper : IPatchDbDepartmentMapper
  {
    public JsonPatchDocument<DbDepartment> Map(JsonPatchDocument<EditDepartmentRequest> request)
    {
      if (request is null)
      {
        return null;
      }

      JsonPatchDocument<DbDepartment> result = new();

      foreach (Operation<EditDepartmentRequest> item in request.Operations)
      {
        result.Operations.Add(new Operation<DbDepartment>(item.op, item.path, item.from, item.value));
      }

      return result;
    }
  }
}
