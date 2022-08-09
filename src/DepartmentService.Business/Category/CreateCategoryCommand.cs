using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation.Results;
using LT.DigitalOffice.DepartmentService.Business.Category.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Category;
using LT.DigitalOffice.DepartmentService.Validation.Category.Interfaces;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.DepartmentService.Business.Category
{
  public class CreateCategoryCommand : ICreateCategoryCommand
  {
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDbCategoryMapper _mapper;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICreateCategoryRequestValidator _validator;
    private readonly IResponseCreator _responseCreator;
    private readonly IAccessValidator _accessValidator;

    public CreateCategoryCommand(
      IHttpContextAccessor httpContextAccessor,
      IDbCategoryMapper mapper,
      ICategoryRepository categoryRepository,
      ICreateCategoryRequestValidator validator,
      IResponseCreator responseCreator,
      IAccessValidator accessValidator)
    {
      _httpContextAccessor = httpContextAccessor;
      _mapper = mapper;
      _categoryRepository = categoryRepository;
      _validator = validator;
      _responseCreator = responseCreator;
      _accessValidator = accessValidator;
    }

    public async Task<OperationResultResponse<Guid?>> ExecuteAsync(CreateCategoryRequest request)
    {
      if (!await _accessValidator.HasRightsAsync(Rights.AddEditRemoveDepartments))
      {
        return _responseCreator.CreateFailureResponse<Guid?>(HttpStatusCode.Forbidden);
      }

      ValidationResult validationResult = await _validator.ValidateAsync(request);

      if (!validationResult.IsValid)
      {
        return _responseCreator.CreateFailureResponse<Guid?>(HttpStatusCode.BadRequest,
          validationResult.Errors.Select(vf => vf.ErrorMessage).ToList());
      }

      DbCategory dbCategory = _mapper.Map(request);

      await _categoryRepository.CreateAsync(dbCategory);

      OperationResultResponse<Guid?> response = new(body: dbCategory.Id);
      _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;

      return response.Body == default
        ? _responseCreator.CreateFailureResponse<Guid?>(HttpStatusCode.BadRequest)
        : response;
    }
  }
}
