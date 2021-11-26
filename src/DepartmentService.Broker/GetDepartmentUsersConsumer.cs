using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Models.Broker.Requests.Department;
using LT.DigitalOffice.Models.Broker.Responses.Department;
using MassTransit;

namespace LT.DigitalOffice.DepartmentService.Broker
{
  public class GetDepartmentUsersConsumer : IConsumer<IGetDepartmentUsersRequest>
  {
    private readonly IDepartmentUserRepository _repository;

    private async Task<object> FindUsersAsync(IGetDepartmentUsersRequest request)
    {
      (List<Guid> userIds, int totalCount) = await _repository.GetAsync(request);

      return IGetDepartmentUsersResponse.CreateObj(userIds, totalCount);
    }

    public GetDepartmentUsersConsumer(
      IDepartmentUserRepository repository)
    {
      _repository = repository;
    }

    public async Task Consume(ConsumeContext<IGetDepartmentUsersRequest> context)
    {
      object result = OperationResultWrapper.CreateResponse(FindUsersAsync, context.Message);

      await context.RespondAsync<IOperationResult<IGetDepartmentUsersResponse>>(result);
    }
  }
}
