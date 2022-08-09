using System;
using LT.DigitalOffice.DepartmentService.Mappers.Db.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Category;
using LT.DigitalOffice.Kernel.Extensions;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.DepartmentService.Mappers.Db
{
  public class DbCategoryMapper : IDbCategoryMapper
  {
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DbCategoryMapper(IHttpContextAccessor httpContextAccessor)
    {
      _httpContextAccessor = httpContextAccessor;
    }

    public DbCategory Map(CreateCategoryRequest request)
    {
      return request is null
        ? null
        : new DbCategory
        {
          Id = Guid.NewGuid(),
          Name = request.Name,
          CreatedBy = _httpContextAccessor.HttpContext.GetUserId(),
          CreatedAtUtc = DateTime.UtcNow,
        };
    }
  }
}
