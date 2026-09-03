using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Organization;

namespace HRAttendance.Business.Interfaces;

public interface IOrganizationService
{
    Task<ApiResponseDto<OrganizationDto>> GetOrganizationByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<List<DepartmentDto>>> GetDepartmentsAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<List<DesignationDto>>> GetDesignationsAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<List<LocationDto>>> GetLocationsAsync(int organizationId, CancellationToken cancellationToken = default);
}
