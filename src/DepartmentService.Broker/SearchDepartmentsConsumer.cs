using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Data.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Requests.Company;
using LT.DigitalOffice.Models.Broker.Responses.Search;
using MassTransit;

namespace LT.DigitalOffice.DepartmentService.Broker
{
  public class SearchDepartmentsConsumer : IConsumer<ISearchDepartmentsRequest>
  {
    private IDepartmentRepository _repository;

    private object SearchDepartment(string text)
    {
      List<DbDepartment> departments = _repository.Search(text);

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
