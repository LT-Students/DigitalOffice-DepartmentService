using System;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Category;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;

namespace LT.DigitalOffice.DepartmentService.Business.Category.Interfaces
{
  [AutoInject]
  public interface ICreateCategoryCommand
  {
    Task<OperationResultResponse<Guid?>> ExecuteAsync(CreateCategoryRequest request);
  }
}
