using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LT.DigitalOffice.DepartmentService.Models.Db
{
  public class DbDepartmentUser
  {
    public const string TableName = "DepartmentsUsers";

    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid DepartmentId { get; set; }
    public int Role { get; set; }
    public bool IsActive { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public DateTime? LeftAtUts { get; set; }
    public DbDepartment Department { get; set; }
  }

  public class DbDepartmentUserConfiguration : IEntityTypeConfiguration<DbDepartmentUser>
  {
    public void Configure(EntityTypeBuilder<DbDepartmentUser> builder)
    {
      builder
        .ToTable(DbDepartmentUser.TableName);

      builder
        .HasKey(u => u.Id);

      builder
        .HasOne(u => u.Department)
        .WithMany(d => d.Users)
        .HasForeignKey(u => u.DepartmentId);
    }
  }
}
