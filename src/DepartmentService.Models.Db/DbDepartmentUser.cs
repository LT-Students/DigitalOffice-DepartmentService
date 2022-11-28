using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LT.DigitalOffice.DepartmentService.Models.Db
{
  public class DbDepartmentUser
  {
    public const string TableName = "DepartmentsUsers";
    public const string HistoryTableName = "DepartmentsUsersHistory";

    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid DepartmentId { get; set; }
    public int Role { get; set; }
    public int Assignment { get; set; }
    public bool IsActive { get; set; }
    public bool IsPending { get; set; }
    public Guid CreatedBy { get; set; }

    public DbDepartment Department { get; set; }
  }

  public class DbDepartmentUserConfiguration : IEntityTypeConfiguration<DbDepartmentUser>
  {
    public void Configure(EntityTypeBuilder<DbDepartmentUser> builder)
    {
      builder
        .ToTable(DbDepartmentUser.TableName, du => du.IsTemporal(h =>
        {
          h.UseHistoryTable(DbDepartmentUser.HistoryTableName);
        }));

      builder
        .HasKey(u => u.Id);

      builder
        .HasOne(u => u.Department)
        .WithMany(d => d.Users);
    }
  }
}
