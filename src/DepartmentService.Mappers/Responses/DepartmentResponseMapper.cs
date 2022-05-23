using System;
using System.Collections.Generic;
using System.Linq;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Responses.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.DepartmentService.Models.Dto.Responses;

namespace LT.DigitalOffice.CompanyService.Mappers.Responses
{
  public class DepartmentResponseMapper : IDepartmentResponseMapper
  {
    private readonly IDepartmentInfoMapper _departmentInfoMapper;

    public DepartmentResponseMapper(IDepartmentInfoMapper departmentInfoMapper)
    {
      _departmentInfoMapper = departmentInfoMapper;
    }

    public DepartmentResponse Map(
      DbDepartment dbDepartment,
      IEnumerable<DepartmentUserInfo> users,
      IEnumerable<ProjectInfo> projects)
    {
      if (dbDepartment is null)
      {
        return null;
      }

      Guid? directorUserId = dbDepartment.Users
        ?.FirstOrDefault(u => u.Role == (int)DepartmentUserRole.Manager)
        ?.UserId;

      return new DepartmentResponse
      {
        Department = _departmentInfoMapper.Map(
          dbDepartment,
          directorUserId is null ? null : users.FirstOrDefault(u => u.User.Id == directorUserId)),
        Users = users,
        Projects = projects
      };
    }
  }
}
