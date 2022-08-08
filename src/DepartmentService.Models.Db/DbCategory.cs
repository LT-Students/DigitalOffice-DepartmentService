using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LT.DigitalOffice.DepartmentService.Models.Db
{
  public class DbCategory
  {
    public const string TableName = "Categories";

    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public ICollection<DbDepartment> Departments { get; set; }

    public DbCategory()
    {
      Departments = new HashSet<DbDepartment>();
    }
  }

  public class TypeConfiguration : IEntityTypeConfiguration<DbCategory>
  {
    public void Configure(EntityTypeBuilder<DbCategory> builder)
    {
      builder
        .ToTable(DbCategory.TableName);

      builder
        .HasKey(a => a.Id);

      builder
        .Property(a => a.Name)
        .IsRequired();

      builder
        .HasMany(a => a.Departments)
        .WithOne(r => r.Category);
    }
  }
}
