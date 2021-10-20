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
      IEnumerable<UserInfo> users,
      IEnumerable<ProjectInfo> projects)
    {
      if (dbDepartment == null)
      {
        return null;
      }

      DbDepartmentUser departmentDirector =
        dbDepartment.Users.FirstOrDefault(u => u.Role == (int)DepartmentUserRole.Director && u.DepartmentId == dbDepartment.Id);

      return new DepartmentResponse
      {
        Department = _departmentInfoMapper.Map(dbDepartment, users?.FirstOrDefault(u => u.Id == departmentDirector.UserId)),
        Users = users,
        Projects = projects,
      };
    }
  }
}
