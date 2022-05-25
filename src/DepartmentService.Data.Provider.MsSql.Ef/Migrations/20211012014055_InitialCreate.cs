using System;
using LT.DigitalOffice.DepartmentService.Models.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LT.DigitalOffice.DepartmentService.Data.Provider.MsSql.Ef.Migrations
{
  [DbContext(typeof(DepartmentServiceDbContext))]
  [Migration("20211012014055_InitialCreate")]

  public class InitialCreate : Migration
  {
    #region Create tables

    private void CreateTableDeparments(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: DbDepartment.TableName,
        columns: table => new
        {
          Id = table.Column<Guid>(nullable: false),
          Name = table.Column<string>(nullable: false),
          ShortName = table.Column<string>(nullable: false),
          Description = table.Column<string>(nullable: true),
          IsActive = table.Column<bool>(nullable: false),
          CreatedBy = table.Column<Guid>(nullable: false),
          CreatedAtUtc = table.Column<DateTime>(nullable: false),
          ModifiedBy = table.Column<Guid>(nullable: true),
          ModifiedAtUtc = table.Column<DateTime>(nullable: true)
        },
        constraints: table =>
        {
          table.PrimaryKey($"PK_{DbDepartment.TableName}", x => x.Id);
          table.UniqueConstraint($"UX_{nameof(DbDepartment.Name)}_unique", x => x.Name);
          table.UniqueConstraint($"UX_{nameof(DbDepartment.ShortName)}_unique", x => x.Name);
        });
    }

    private void CreateTableDeparmentsUsers(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: DbDepartmentUser.TableName,
        columns: table => new
        {
          Id = table.Column<Guid>(nullable: false),
          UserId = table.Column<Guid>(nullable: false),
          DepartmentId = table.Column<Guid>(nullable: false),
          Role = table.Column<int>(nullable: false),
          Assignment = table.Column<int>(nullable: false),
          IsActive = table.Column<bool>(nullable: false),
          CreatedBy = table.Column<Guid>(nullable: false),
          PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart"),
          PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart")
        },
        constraints: table =>
        {
          table.PrimaryKey($"PK_{DbDepartmentUser.TableName}", x => x.Id);
        })
        .Annotation("SqlServer:IsTemporal", true)
        .Annotation("SqlServer:TemporalHistoryTableName", $"{DbDepartmentUser.TableName}History")
        .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
        .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");
    }

    private void CreateTableDeparmentsProjects(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: DbDepartmentProject.TableName,
        columns: table => new
        {
          Id = table.Column<Guid>(nullable: false),
          ProjectId = table.Column<Guid>(nullable: false),
          DepartmentId = table.Column<Guid>(nullable: false),
          IsActive = table.Column<bool>(nullable: false),
          CreatedBy = table.Column<Guid>(nullable: false),
          PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart"),
          PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart")
        },
        constraints: table =>
        {
          table.PrimaryKey($"PK_{DbDepartmentProject.TableName}", x => x.Id);
        })
        .Annotation("SqlServer:IsTemporal", true)
        .Annotation("SqlServer:TemporalHistoryTableName", $"{DbDepartmentProject.TableName}History")
        .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
        .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");
    }

    #endregion

    protected override void Up(MigrationBuilder migrationBuilder)
    {
      CreateTableDeparments(migrationBuilder);

      CreateTableDeparmentsUsers(migrationBuilder);

      CreateTableDeparmentsProjects(migrationBuilder);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(DbDepartmentUser.TableName);

      migrationBuilder.DropTable(DbDepartmentProject.TableName);

      migrationBuilder.DropTable(DbDepartment.TableName);
    }
  }
}
