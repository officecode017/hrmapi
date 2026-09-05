using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Organization;
using HRAttendance.Data.DTOs.Department;
using HRAttendance.Data.DTOs.Designation;
using HRAttendance.Data.DTOs.Location;

namespace HRAttendance.Business.Interfaces;

public interface IOrganizationService
{
    Task<ApiResponseDto<OrganizationDto>> GetOrganizationByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<OrganizationBrandingDto>> GetOrganizationBrandingAsync(int? organizationId = null, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<List<DepartmentDto>>> GetDepartmentsAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<List<DesignationDto>>> GetDesignationsAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<List<LocationDto>>> GetLocationsAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> UpdateOrganizationAsync(int id, UpdateOrganizationDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<OrganizationLogoResponseDto>> UpdateLogoAsync(int id, Microsoft.AspNetCore.Http.IFormFile file, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> RemoveLogoAsync(int id, CancellationToken cancellationToken = default);
}
