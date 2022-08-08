using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.DepartmentService.Models.Db
{
  public class DbDepartment
  {
    public const string TableName = "Departments";

    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public Guid? CategoryId { get; set; }
    public string Name { get; set; }
    public string ShortName { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }

    public DbCategory Category { get; set; }

    public ICollection<DbDepartmentUser> Users { get; set; }
    public ICollection<DbDepartmentProject> Projects { get; set; }

    public DbDepartment()
    {
      Users = new HashSet<DbDepartmentUser>();

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
        .Property(d => d.Name)
        .IsRequired();

      builder
        .Property(d => d.ShortName)
        .IsRequired();

      builder
        .HasMany(d => d.Users)
        .WithOne(u => u.Department);

      builder
        .HasMany(d => d.Projects)
        .WithOne(u => u.Department);

      builder
        .HasOne(d => d.Category)
        .WithMany(u => u.Departments);
    }
  }
}
