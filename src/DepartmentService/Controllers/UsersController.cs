using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.User.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.DepartmentUser;
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
      [FromBody] CreateDepartmentUsersRequest request)
    {
      return await command.ExecuteAsync(request);
    }

    [HttpDelete("remove")]
    public async Task<OperationResultResponse<bool>> RemoveAsync(
      [FromServices] IRemoveDepartmentUsersCommand command,
      [FromQuery] Guid departmentId,
      [FromBody] List<Guid> usersIds)
    {
      return await command.ExecuteAsync(departmentId, usersIds);
    }

    [HttpPut("editRole")]
    public async Task<OperationResultResponse<bool>> EditRoleAsync(
      [FromServices] IEditDepartmentUsersRoleCommand command,
      [FromQuery] Guid departmentId,
      [FromBody] EditDepartmentUserRoleRequest request)
    {
      return await command.ExecuteAsync(departmentId, request);
    }

    [HttpPut("editAssignment")]
    public async Task<OperationResultResponse<bool>> EditAssignmentAsync(
      [FromServices] IEditDepartmentUsersAssigmentCommand command,
      [FromQuery] Guid departmentId,
      [FromBody] EditDepartmentUserAssignmentRequest request)
    {
      return await command.ExecuteAsync(departmentId, request);
    }
  }
}
