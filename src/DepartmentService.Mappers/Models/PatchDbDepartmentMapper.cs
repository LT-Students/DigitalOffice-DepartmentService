using System;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace LT.DigitalOffice.DepartmentService.Mappers.Models
{
  public class PatchDbDepartmentMapper : IPatchDbDepartmentMapper
  {
    public JsonPatchDocument<DbDepartment> Map(JsonPatchDocument<EditDepartmentRequest> request)
    {
      if (request == null)
      {
        return null;
      }

      JsonPatchDocument<DbDepartment> result = new JsonPatchDocument<DbDepartment>();

      foreach (Operation<EditDepartmentRequest> item in request.Operations)
      {
        if (item.path.EndsWith(nameof(EditDepartmentRequest.DirectorId), StringComparison.OrdinalIgnoreCase))
        {
          continue;
        }

        result.Operations.Add(new Operation<DbDepartment>(item.op, item.path, item.from, item.value));
      }

      return result;
    }
  }
}
