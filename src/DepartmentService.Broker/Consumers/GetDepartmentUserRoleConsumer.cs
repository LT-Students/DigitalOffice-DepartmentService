using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Broker.Helpers.Branch.Interfaces;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Requests.Department;
using LT.DigitalOffice.Models.Broker.Responses.Department;
using MassTransit;

namespace LT.DigitalOffice.DepartmentService.Broker.Consumers
{
  public class GetDepartmentUserRoleConsumer : IConsumer<IGetDepartmentUserRoleRequest>
  {
    private readonly IDepartmentUserRepository _departmentUserRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IDepartmentChildren _departmentChildren;

    private async Task<object> CheckUserRoleAsync(IGetDepartmentUserRoleRequest request)
    {
      DbDepartmentUser departmentUser = await _departmentUserRepository.GetAsync(request.UserId);;

      if (departmentUser.DepartmentId == request.DepartmentId)
      {
        return IGetDepartmentUserRoleResponse.CreateObj((DepartmentUserRole?)departmentUser.Role);
      }

      if (departmentUser.DepartmentId != request.DepartmentId)
      {
        List<Guid> departmentsChildrenIds = new();

        _departmentChildren.GetChildrenIds(await _departmentRepository.GetDepartmentsTreeAsync(new()), departmentUser.DepartmentId, departmentsChildrenIds);

        if (departmentsChildrenIds is not null && departmentsChildrenIds.Contains(request.DepartmentId))
        {
          return IGetDepartmentUserRoleResponse.CreateObj((DepartmentUserRole?)departmentUser.Role);
        }
      }

      return null;
    }

    public GetDepartmentUserRoleConsumer(
      IDepartmentUserRepository departmentUserRepository,
      IDepartmentRepository departmentRepository,
      IDepartmentChildren departmentChildren)
    {
      _departmentUserRepository = departmentUserRepository;
      _departmentRepository = departmentRepository;
      _departmentChildren = departmentChildren;
    }

    public async Task Consume(ConsumeContext<IGetDepartmentUserRoleRequest> context)
    {
      object response = OperationResultWrapper.CreateResponse(CheckUserRoleAsync, context.Message);

      await context.RespondAsync<IOperationResult<IGetDepartmentUserRoleResponse>>(response);
    }
  }
}
