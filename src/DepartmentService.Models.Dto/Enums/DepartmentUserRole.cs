using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Enums
{
  [JsonConverter(typeof(StringEnumConverter))]
  public enum DepartmentUserRole
  {
    Employee,
    Director
  }
}
