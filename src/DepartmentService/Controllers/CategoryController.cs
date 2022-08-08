using System;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.Category.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Category;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Category.Filters;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.DepartmentService.Controllers
{
  [Route("[controller]")]
  [ApiController]
  public class CategoryController : ControllerBase
  {
    [HttpPost("create")]
    public async Task<OperationResultResponse<Guid?>> CreateAsync(
      [FromServices] ICreateCategoryCommand command,
      [FromBody] CreateCategoryRequest request)
    {
      return await command.ExecuteAsync(request);
    }

    [HttpGet("find")]
    public async Task<FindResultResponse<CategoryInfo>> FindAsync(
      [FromServices] IFindCategoryCommand command,
      [FromQuery] FindCategoriesFilter request)
    {
      return await command.ExecuteAsync(request);
    }

    [HttpPost("remove")]
    public async Task<OperationResultResponse<bool>> RemoveAsync(
      [FromServices] IRemoveCategoryCommand command,
      [FromBody] RemoveCategoryRequest request)
    {
      return await command.ExecuteAsync(request);
    }
  }
}
