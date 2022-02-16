using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Models.Broker.Models.Department;
using LT.DigitalOffice.Models.Broker.Requests.Department;
using LT.DigitalOffice.Models.Broker.Responses.Department;
using MassTransit;

namespace LT.DigitalOffice.DepartmentService.Broker
{
  public class FilterDepartmentsUsersConsumer : IConsumer<IFilterDepartmentsRequest>
  {
    private readonly IDepartmentRepository _repository;

    public async Task<List<DepartmentFilteredData>> GetDepartmentFilteredData(IFilterDepartmentsRequest request)
    {
      List<DbDepartment> dbPosition = await _repository.GetAsync(request.DepartmentsIds);

      return dbPosition.Select(
        pd => new DepartmentFilteredData(
          pd.Id,
          pd.Name,
          pd.Users.Select(u => u.UserId).ToList()))
        .ToList();
    }

    public FilterDepartmentsUsersConsumer(
      IDepartmentRepository repository)
    {
      _repository = repository;
    }

    public async Task Consume(ConsumeContext<IFilterDepartmentsRequest> context)
    {
      List<DepartmentFilteredData> departmentFilteredData = await GetDepartmentFilteredData(context.Message);

      await context.RespondAsync<IOperationResult<IFilterDepartmentsResponse>>(
        OperationResultWrapper.CreateResponse((_) => IFilterDepartmentsResponse.CreateObj(departmentFilteredData), context));
    }
  }
}
