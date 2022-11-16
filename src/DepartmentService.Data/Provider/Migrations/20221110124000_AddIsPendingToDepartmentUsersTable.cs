using LT.DigitalOffice.DepartmentService.Models.Db;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LT.DigitalOffice.DepartmentService.Data.Provider.Migrations
{
  [DbContext(typeof(DepartmentServiceDbContext))]
  [Migration("20221110124000_AddIsPendingToDepartmentUsersTable")]

  public class AddIsPendingToDepartmentUsersTable : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<bool>(
        name: "IsPending",
        table: DbDepartmentUser.TableName,
        defaultValue: false,
        nullable: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
        name: "IsPending",
        table: DbDepartmentUser.TableName);
    }
  }
}
