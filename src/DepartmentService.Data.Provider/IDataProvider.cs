﻿using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Database;
using LT.DigitalOffice.Kernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace LT.DigitalOffice.DepartmentService.Data.Provider
{
  [AutoInject(InjectType.Scoped)]
  public interface IDataProvider : IBaseDataProvider
  {
    DbSet<DbDepartment> Departments { get; set; }

    DbSet<DbDepartmentUser> DepartmentsUsers { get; set; }

    DbSet<DbDepartmentProject> DepartmentsProjects { get; set; }
  }
}
