using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.DepartmentsTree.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.DepartmentService.Controllers
{
  [Route("[controller]")]
  [ApiController]
  public class DepartmentsTreeController : ControllerBase
  {
    [HttpGet("get")]
    public async Task<OperationResultResponse<List<DepartmentsTreeInfo>>> GetDepartmentsTreeAsync(
      [FromServices] IGetDepartmentsTreeCommand command)
    {
      return await command.ExecuteAsync();
    }
  }
}
