using System;

namespace LT.DigitalOffice.DepartmentService.Models.Dto.Models
{
  public record  NewsInfo
  {
    public Guid Id { get; set; }
    public string Preview { get; set; }
    public string Subject { get; set; }
    public string Pseudonym { get; set; }
    public UserInfo Author { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public UserInfo Sender { get; set; }
  }
} 
