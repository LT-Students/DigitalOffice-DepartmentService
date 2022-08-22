using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Category.Filters;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;

namespace LT.DigitalOffice.DepartmentService.Business.Category.Interfaces
{
  [AutoInject]
  public interface IFindCategoryCommand
  {
    Task<FindResultResponse<CategoryInfo>> ExecuteAsync(FindCategoriesFilter filter);
  }
}
