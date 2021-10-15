using System;
using System.Collections.Generic;
using System.Linq;
using LT.DigitalOffice.DepartmentService.Mappers.Models.Interfaces;
using LT.DigitalOffice.DepartmentService.Mappers.Responses.Interfaces;
using LT.DigitalOffice.DepartmentService.Models.Db;
using LT.DigitalOffice.DepartmentService.Models.Dto.Enums;
using LT.DigitalOffice.DepartmentService.Models.Dto.Models;
using LT.DigitalOffice.DepartmentService.Models.Dto.Requests.Filters;
using LT.DigitalOffice.DepartmentService.Models.Dto.Responses;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Company;

namespace LT.DigitalOffice.CompanyService.Mappers.Responses
{
  public class DepartmentResponseMapper : IDepartmentResponseMapper
  {
    private readonly IProjectInfoMapper _projectInfoMapper;
    private readonly IDepartmentUserInfoMapper _departmentUserInfoMapper;
    private readonly IDepartmentInfoMapper _departmentInfoMapper;

    private ImageData GetImage(List<ImageData> images, Guid? imageId)
    {
      if (images == null || !images.Any() || !imageId.HasValue)
      {
        return null;
      }

      return images.FirstOrDefault(
        i =>
          i.ParentId == imageId ||
          i.ImageId == imageId);
    }

    public DepartmentResponseMapper(
      IProjectInfoMapper projectInfoMapper,
      IDepartmentUserInfoMapper departmentUserInfoMapper,
      IDepartmentInfoMapper departmentInfoMapper)
    {
      _projectInfoMapper = projectInfoMapper;
      _departmentUserInfoMapper = departmentUserInfoMapper;
      _departmentInfoMapper = departmentInfoMapper;
    }

    public DepartmentResponse Map(
      DbDepartment dbDepartment,
      List<UserData> usersData,
      List<PositionData> positionData,
      List<ImageData> userImages,
      List<ProjectData> projectsInfo,
      GetDepartmentFilter filter)
    {
      DepartmentInfo department;

      DbDepartmentUser departmentDirector =
        dbDepartment.Users.FirstOrDefault(u => u.Role == (int)DepartmentUserRole.Director && u.DepartmentId == dbDepartment.Id);

      if (departmentDirector != null)
      {
        UserData director = usersData?.FirstOrDefault(ud => ud.Id == departmentDirector.UserId);

        department = _departmentInfoMapper.Map(
          dbDepartment,
          _departmentUserInfoMapper.Map(
            director,
            positionData?.FirstOrDefault(pu => pu.UsersIds.FirstOrDefault() == departmentDirector.UserId),
            GetImage(userImages, director?.ImageId)));

        usersData = usersData?.Where(ud => ud.Id != director?.Id).ToList();
      }
      else
      {
        department = _departmentInfoMapper.Map(dbDepartment, null);
      }

      return new DepartmentResponse
      {
        Department = department,
        Users = filter.IsIncludeUsers ?
          usersData?.Select(ud =>
            _departmentUserInfoMapper.Map(
              ud,
              positionData?.FirstOrDefault(pu => pu.UsersIds.FirstOrDefault() == ud.Id),
              GetImage(userImages, ud.ImageId)))
          : null,
        Projects = filter.IsIncludeProjects ? projectsInfo?.Select(pi => _projectInfoMapper.Map(pi)) : null
      };
    }
  }
}
