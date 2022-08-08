using System.Reflection;
using System.Threading.Tasks;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.Kernel.EFSupport.Provider;
using Microsoft.EntityFrameworkCore;

namespace LT.DigitalOffice.DepartmentService.Data.Provider.MsSql.Ef
{
  public class DepartmentServiceDbContext : DbContext, IDataProvider
  {
    public DbSet<DbDepartment> Departments { get; set; }
    public DbSet<DbDepartmentUser> DepartmentsUsers { get; set; }
    public DbSet<DbDepartmentProject> DepartmentsProjects { get; set; }
    public DbSet<DbCategory> Categories { get; set; }

    public DepartmentServiceDbContext(DbContextOptions<DepartmentServiceDbContext> options)
      : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("LT.DigitalOffice.DepartmentService.Models.Db"));
    }

    public object MakeEntityDetached(object obj)
    {
      Entry(obj).State = EntityState.Detached;
      return Entry(obj).State;
    }

    public void EnsureDeleted()
    {
      Database.EnsureDeleted();
    }

    public bool IsInMemory()
    {
      return Database.IsInMemory();
    }

    public int ExecuteRawSql(string query)
    {
      return Database.ExecuteSqlRaw(query);
    }

    void IBaseDataProvider.Save()
    {
      SaveChanges();
    }

    async Task IBaseDataProvider.SaveAsync()
    {
      await SaveChangesAsync();
    }
  }
}
