using System;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Filters;
using LT.DigitalOffice.DepartmentService.Models.Dto.Responses;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.DepartmentService.Controllers
{
  [Route("[controller]")]
  [ApiController]
  public class DepartmentController : ControllerBase
  {
    [HttpPost("create")]
    public async Task<OperationResultResponse<Guid>> Create(
      [FromServices] ICreateDepartmentCommand command,
      [FromBody] CreateDepartmentRequest department)
    {
      return await command.ExecuteAsync(department);
    }

    [HttpGet("get")]
    public async Task<OperationResultResponse<DepartmentResponse>> Get(
      [FromServices] IGetDepartmentCommand command,
      [FromQuery] GetDepartmentFilter filter)
    {
      return await command.ExecuteAsync(filter);
    }

    [HttpGet("find")]
    public async Task<FindResultResponse<DepartmentInfo>> Find(
      [FromServices] IFindDepartmentsCommand command,
      [FromQuery] FindDepartmentFilter filter)
    {
      return await command.ExecuteAsync(filter);
    }

    [HttpPatch("edit")]
    public async Task<OperationResultResponse<bool>> Edit(
      [FromServices] IEditDepartmentCommand command,
      [FromQuery] Guid departmentId,
      [FromBody] JsonPatchDocument<EditDepartmentRequest> request)
    {
      return await command.ExecuteAsync(departmentId, request);
    }
  }
}
