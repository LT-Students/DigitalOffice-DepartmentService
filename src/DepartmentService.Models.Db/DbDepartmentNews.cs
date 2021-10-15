using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LT.DigitalOffice.DepartmentService.Models.Db
{
  public class DbDepartmentNews
  {
    public const string TableName = "DepartmentsNews";

    public Guid Id { get; set; }
    public Guid NewsId { get; set; }
    public Guid DepartmentId { get; set; }
    public bool IsActive { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public DbDepartment Department { get; set; }
  }

  public class DbDepartmentNewsConfiguration : IEntityTypeConfiguration<DbDepartmentNews>
  {
    public void Configure(EntityTypeBuilder<DbDepartmentNews> builder)
    {
      builder
        .ToTable(DbDepartmentNews.TableName);

      builder
        .HasKey(u => u.Id);

      builder
        .HasOne(u => u.Department)
        .WithMany(d => d.News);
    }
  }
}
