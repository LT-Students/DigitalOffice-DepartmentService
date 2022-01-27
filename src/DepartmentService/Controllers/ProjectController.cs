using System;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.Project.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.DepartmentService.Controllers
{
  [Route("[controller]")]
  [ApiController]
  public class ProjectController : ControllerBase
  {
    [HttpPut("edit")]
    public async Task<OperationResultResponse<Guid?>> EditAsync(
      [FromServices] IEditDepartmentProjectCommand command,
      [FromBody] EditDepartmentProjectRequest request)
    {
      return await command.ExecuteAsync(request);
    }
  }
}
