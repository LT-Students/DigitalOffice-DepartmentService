using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.User.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.DepartmentService.Controllers
{
  [ApiController]
  [Route("[Controller]")]
  public class UsersController : ControllerBase
  {
    [HttpPost("create")]
    public async Task<OperationResultResponse<bool>> CreateAsync(
      [FromServices] ICreateDepartmentUsersCommand command,
      [FromQuery] Guid departmentId,
      [FromBody] List<Guid> usersIds)
    {
      return await command.ExecuteAsync(departmentId, usersIds);
    }

    [HttpDelete("remove")]
    public async Task<OperationResultResponse<bool>> RemoveAsync(
      [FromServices] IRemoveDepartmentUsersCommand command,
      [FromQuery] Guid departmentId,
      [FromBody] List<Guid> usersIds)
    {
      return await command.ExecuteAsync(departmentId, usersIds);
    }
  }
}
