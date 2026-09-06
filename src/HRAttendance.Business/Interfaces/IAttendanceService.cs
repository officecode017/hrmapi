using HRAttendance.Data.DTOs.Attendance;
using HRAttendance.Data.DTOs.Common;

namespace HRAttendance.Business.Interfaces;

public interface IAttendanceService
{
    Task<ApiResponseDto<AttendanceDto>> CheckInAsync(CheckInRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<AttendanceDto>> CheckOutAsync(CheckOutRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<List<AttendanceHistoryDto>>> GetHistoryAsync(int employeeId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<AttendanceDto?>> GetTodayStatusAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<AttendanceDto>> AdminMarkAttendanceAsync(AdminMarkAttendanceRequestDto request, int adminUserId, CancellationToken cancellationToken = default);
}
