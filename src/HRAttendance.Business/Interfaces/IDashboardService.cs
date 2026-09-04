using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Dashboard;

namespace HRAttendance.Business.Interfaces;

public interface IDashboardService
{
    Task<ApiResponseDto<EmployeeDashboardDto>> GetEmployeeDashboardAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<EmployeeCalendarDto>> GetEmployeeCalendarAsync(int employeeId, int year, int month, CancellationToken cancellationToken = default);
}
