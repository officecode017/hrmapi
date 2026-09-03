using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Shift;
using HRAttendance.Data.DTOs.Holiday;
using HRAttendance.Data.DTOs.Role;
using HRAttendance.Data.DTOs.Notification;
using HRAttendance.Data.DTOs.Overtime;

namespace HRAttendance.Business.Interfaces;

public interface IShiftService
{
    Task<ApiResponseDto<List<ShiftDto>>> GetShiftsAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<ShiftDto>> CreateShiftAsync(CreateShiftDto request, CancellationToken cancellationToken = default);
}

public interface IHolidayService
{
    Task<ApiResponseDto<List<HolidayDto>>> GetHolidaysAsync(int organizationId, int academicYearId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<HolidayDto>> CreateHolidayAsync(CreateHolidayDto request, CancellationToken cancellationToken = default);
}

public interface IRoleService
{
    Task<ApiResponseDto<List<RoleDto>>> GetRolesAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> AssignRoleAsync(AssignRoleDto request, CancellationToken cancellationToken = default);
}

public interface INotificationService
{
    Task<ApiResponseDto<List<NotificationDto>>> GetNotificationsAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default);
}

public interface IOvertimeService
{
    Task<ApiResponseDto<List<OTEntryDto>>> GetEntriesAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<OTEntryDto>> CalculateOvertimeAsync(CalculateOvertimeRequestDto request, CancellationToken cancellationToken = default);
}
