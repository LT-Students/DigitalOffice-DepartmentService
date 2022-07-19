using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Configurations
{
  public record MemoryCacheConfig
  {
    public const string SectionName = "MemoryCache";

    public double CacheLiveInMinutes { get; set; }
  }
}
