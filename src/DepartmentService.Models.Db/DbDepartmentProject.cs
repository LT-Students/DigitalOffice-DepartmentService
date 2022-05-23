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
    public bool IsActive { get; set; }
    public Guid CreatedBy { get; set; }

    public DbDepartment Department { get; set; }
  }

  public class DbDepartmentProjectConfiguration : IEntityTypeConfiguration<DbDepartmentProject>
  {
    public void Configure(EntityTypeBuilder<DbDepartmentProject> builder)
    {
      builder
        .ToTable(DbDepartmentProject.TableName, dp => dp.IsTemporal());

      builder
        .HasKey(u => u.Id);

      builder
        .HasOne(u => u.Department)
        .WithMany(d => d.Projects);
    }
  }
}
