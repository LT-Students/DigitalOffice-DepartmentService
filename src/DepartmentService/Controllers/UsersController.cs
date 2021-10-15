using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests;
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
  }
}
