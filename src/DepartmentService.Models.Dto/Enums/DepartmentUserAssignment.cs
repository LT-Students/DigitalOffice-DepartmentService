using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Enums
{
  [JsonConverter(typeof(StringEnumConverter))]
  public enum DepartmentUserAssignment
  {
    Employee = 0,
    Director = 10
  }
}
