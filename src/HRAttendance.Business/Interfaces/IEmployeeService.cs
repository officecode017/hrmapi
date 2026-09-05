using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Employee;

namespace HRAttendance.Business.Interfaces;

public interface IEmployeeService
{
    Task<ApiResponseDto<PagedResponseDto<EmployeeListDto>>> GetEmployeesAsync(int organizationId, PagedRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<EmployeeDto>> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<EmployeeDto>> CreateEmployeeAsync(CreateEmployeeDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> UpdateEmployeeAsync(int id, UpdateEmployeeDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> DeleteEmployeeAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<EmployeePhotoResponseDto>> UpdatePhotoAsync(int id, Microsoft.AspNetCore.Http.IFormFile file, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> RemovePhotoAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<EmployeeBackgroundResponseDto>> UpdateBackgroundAsync(int id, Microsoft.AspNetCore.Http.IFormFile file, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> RemoveBackgroundAsync(int id, CancellationToken cancellationToken = default);
}
