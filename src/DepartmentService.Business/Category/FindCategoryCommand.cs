using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.Category.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Category.Filters;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Kernel.Validators.Interfaces;

namespace LT.DigitalOffice.DepartmentService.Business.Category
{
  public class FindCategoryCommand : IFindCategoryCommand
  {
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICategoryInfoMapper _mapper;
    private readonly IBaseFindFilterValidator _baseFindValidator;
    private readonly IResponseCreator _responseCreator;

    public FindCategoryCommand(
      IBaseFindFilterValidator baseFindValidator,
      ICategoryRepository categoryRepository,
      ICategoryInfoMapper mapper,
      IResponseCreator responseCreator)
    {
      _responseCreator = responseCreator;
      _baseFindValidator = baseFindValidator;
      _categoryRepository = categoryRepository;
      _mapper = mapper;
    }

    public async Task<FindResultResponse<CategoryInfo>> ExecuteAsync(FindCategoriesFilter filter)
    {
      if (!_baseFindValidator.ValidateCustom(filter, out List<string> errors))
      {
        return _responseCreator.CreateFailureFindResponse<CategoryInfo>(HttpStatusCode.BadRequest, errors);
      }

      FindResultResponse<CategoryInfo> response = new();

      (List<DbCategory> dbCategories, int totalCount) = await _categoryRepository.FindAsync(filter);

      response.TotalCount = totalCount;

      response.Body = dbCategories.Select(_mapper.Map).ToList();

      return response;
    }
  }
}
