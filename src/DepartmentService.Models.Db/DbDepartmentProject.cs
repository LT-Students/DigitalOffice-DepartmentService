using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LT.DigitalOffice.DepartmentService.Models.Db
{
  public class DbDepartmentProject
  {
    public const string TableName = "DepartmentsProjects";

    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }

    public DbDepartment Department { get; set; }
  }

  public class DbDepartmentProjectConfiguration : IEntityTypeConfiguration<DbDepartmentProject>
  {
    public void Configure(EntityTypeBuilder<DbDepartmentProject> builder)
    {
      builder
        .ToTable(DbDepartmentProject.TableName);

      builder
        .HasKey(u => u.Id);

      builder
        .HasOne(u => u.Department)
        .WithMany(d => d.Projects);
    }
  }
}
