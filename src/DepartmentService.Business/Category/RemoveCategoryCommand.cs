using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.Category.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Category;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;

namespace LT.DigitalOffice.DepartmentService.Business.Category
{
  public class RemoveCategoryCommand : IRemoveCategoryCommand
  {
    private readonly ICategoryRepository _repository;
    private readonly IAccessValidator _accessValidator;
    private readonly IResponseCreator _responseCreator;

    public RemoveCategoryCommand(
      ICategoryRepository repository,
      IAccessValidator accessValidator,
      IResponseCreator responseCreator)
    {
      _repository = repository;
      _accessValidator = accessValidator;
      _responseCreator = responseCreator;
    }

    public async Task<OperationResultResponse<bool>> ExecuteAsync(RemoveCategoryRequest request)
    {
      if (!await _accessValidator.HasRightsAsync(Rights.AddEditRemoveDepartments))
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.Forbidden);
      }

      OperationResultResponse<bool> response = new();

      response.Body = await _repository.RemoveAsync(request.Id);

      return response;
    }
  }
}
