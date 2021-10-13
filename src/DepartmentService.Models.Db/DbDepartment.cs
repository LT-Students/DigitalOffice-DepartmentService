using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.DepartmentService.Models.Db
{
  public class DbDepartment
  {
    public const string TableName = "Department";

    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }

    public ICollection<DbDepartmentUser> Users { get; set; }
    public ICollection<DbDepartmentNews> News { get; set; }
    public ICollection<DbDepartmentProject> Projects { get; set; }

    public DbDepartment()
    {
      Users = new HashSet<DbDepartmentUser>();

      News = new HashSet<DbDepartmentNews>();

      Projects = new HashSet<DbDepartmentProject>();
    }
  }

  public class DbDepartmentConfiguration : IEntityTypeConfiguration<DbDepartment>
  {
    public void Configure(EntityTypeBuilder<DbDepartment> builder)
    {
      builder
        .ToTable(DbDepartment.TableName);

      builder
        .HasKey(d => d.Id);

      builder
        .HasMany(d => d.Users)
        .WithOne(u => u.Department);

      builder
        .HasMany(d => d.News)
        .WithOne(u => u.Department);

      builder
        .HasMany(d => d.Projects)
        .WithOne(u => u.Department);
    }
  }
}
