﻿using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Business.Department.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Responses.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Department.Filters;
using LT.DigitalOffice.DepartmentService.Models.Dto.Responses;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;

namespace LT.DigitalOffice.DepartmentService.Business.Department
{
  public class GetDepartmentCommand : IGetDepartmentCommand
  {
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IDepartmentResponseMapper _departmentResponseMapper;
    private readonly IResponseCreator _responseCreator;

    public GetDepartmentCommand(
      IDepartmentRepository departmentRepository,
      IDepartmentResponseMapper departmentResponseMapper,
      IResponseCreator responseCreator)
    {
      _departmentRepository = departmentRepository;
      _departmentResponseMapper = departmentResponseMapper;
      _responseCreator = responseCreator;
    }

    public async Task<OperationResultResponse<DepartmentResponse>> ExecuteAsync(GetDepartmentFilter filter, CancellationToken cancellationToken = default)
    {
      DbDepartment dbDepartment = await _departmentRepository.GetAsync(filter, cancellationToken);

      if (dbDepartment is null)
      {
        return _responseCreator.CreateFailureResponse<DepartmentResponse>(HttpStatusCode.NotFound);
      }

      return new OperationResultResponse<DepartmentResponse>(
        body: _departmentResponseMapper.Map(dbDepartment));
    }
  }
}
