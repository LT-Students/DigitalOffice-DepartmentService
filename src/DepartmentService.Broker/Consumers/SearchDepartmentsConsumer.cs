using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Requests.Department;
using LT.DigitalOffice.Models.Broker.Responses.Search;
using MassTransit;

namespace LT.DigitalOffice.DepartmentService.Broker.Consumers
{
  public class SearchDepartmentsConsumer : IConsumer<ISearchDepartmentsRequest>
  {
    private IDepartmentRepository _repository;

    private async Task<object> SearchDepartment(string text)
    {
      List<DbDepartment> departments = await _repository.SearchAsync(text);

      return ISearchResponse.CreateObj(
        departments.Select(
          d => new SearchInfo(d.Id, d.Name)).ToList());
    }

    public SearchDepartmentsConsumer(
      IDepartmentRepository repository)
    {
      _repository = repository;
    }

    public async Task Consume(ConsumeContext<ISearchDepartmentsRequest> context)
    {
      object response = OperationResultWrapper.CreateResponse(SearchDepartment, context.Message.Value);

      await context.RespondAsync<IOperationResult<ISearchResponse>>(response);
    }
  }
}
