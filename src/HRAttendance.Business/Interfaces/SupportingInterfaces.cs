using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Shift;
using HRAttendance.Data.DTOs.Holiday;
using HRAttendance.Data.DTOs.Role;
using HRAttendance.Data.DTOs.Notification;
using HRAttendance.Data.DTOs.Overtime;
using HRAttendance.Data.DTOs.Department;
using HRAttendance.Data.DTOs.Designation;
using HRAttendance.Data.DTOs.Location;
using HRAttendance.Data.DTOs.AcademicYear;
using HRAttendance.Data.DTOs.OffDay;
using HRAttendance.Data.DTOs.Leave;

namespace HRAttendance.Business.Interfaces;

public interface IShiftService
{
    Task<ApiResponseDto<List<ShiftDto>>> GetShiftsAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<ShiftDto>> GetShiftByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<ShiftDto>> CreateShiftAsync(CreateShiftDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> UpdateShiftAsync(int id, UpdateShiftDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> DeleteShiftAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> AssignShiftAsync(AssignShiftDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> BulkAssignShiftAsync(BulkAssignShiftDto request, CancellationToken cancellationToken = default);
}

public interface IHolidayService
{
    Task<ApiResponseDto<List<HolidayDto>>> GetHolidaysAsync(int organizationId, int academicYearId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<HolidayDto>> GetHolidayByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<HolidayDto>> CreateHolidayAsync(CreateHolidayDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> UpdateHolidayAsync(int id, UpdateHolidayDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> DeleteHolidayAsync(int id, CancellationToken cancellationToken = default);
}

public interface IRoleService
{
    Task<ApiResponseDto<List<RoleDto>>> GetRolesAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<RoleDto>> GetRoleByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<RoleDto>> CreateRoleAsync(CreateRoleDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> UpdateRoleAsync(int id, UpdateRoleDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> DeleteRoleAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> AssignRoleAsync(AssignRoleDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<List<PermissionDto>>> GetPermissionsAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> AssignRolePermissionsAsync(AssignRolePermissionsDto request, CancellationToken cancellationToken = default);
}

public interface IOvertimeService
{
    Task<ApiResponseDto<OTSettingDto?>> GetSettingsAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<OTSettingDto>> CreateOTSettingAsync(CreateOTSettingDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> UpdateOTSettingAsync(int id, UpdateOTSettingDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> RecordOTEntryAsync(CreateOTEntryDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<List<OTEntryDto>>> GetPendingOTEntriesAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> ProcessOTApprovalAsync(int id, int approverId, ApproveOTEntryDto request, CancellationToken cancellationToken = default);
}

public interface IDepartmentService
{
    Task<ApiResponseDto<List<DepartmentDto>>> GetDepartmentsAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<DepartmentDto>> GetDepartmentByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<DepartmentDto>> CreateDepartmentAsync(CreateDepartmentDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> UpdateDepartmentAsync(int id, UpdateDepartmentDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> DeleteDepartmentAsync(int id, CancellationToken cancellationToken = default);
}

public interface IDesignationService
{
    Task<ApiResponseDto<List<DesignationDto>>> GetDesignationsAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<DesignationDto>> GetDesignationByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<DesignationDto>> CreateDesignationAsync(CreateDesignationDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> UpdateDesignationAsync(int id, UpdateDesignationDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> DeleteDesignationAsync(int id, CancellationToken cancellationToken = default);
}

public interface ILocationService
{
    Task<ApiResponseDto<List<LocationDto>>> GetLocationsAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<LocationDto>> GetLocationByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<LocationDto>> CreateLocationAsync(CreateLocationDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> UpdateLocationAsync(int id, UpdateLocationDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> DeleteLocationAsync(int id, CancellationToken cancellationToken = default);
}

public interface IAcademicYearService
{
    Task<ApiResponseDto<List<AcademicYearDto>>> GetAcademicYearsAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<AcademicYearDto>> GetAcademicYearByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<AcademicYearDto>> CreateAcademicYearAsync(CreateAcademicYearDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> UpdateAcademicYearAsync(int id, UpdateAcademicYearDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> DeleteAcademicYearAsync(int id, CancellationToken cancellationToken = default);
}

public interface IOffDayService
{
    Task<ApiResponseDto<List<OffDayDto>>> GetOffDaysAsync(int organizationId, int academicYearId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<OffDayDto>> GetOffDayByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<OffDayDto>> CreateOffDayAsync(CreateOffDayDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> UpdateOffDayAsync(int id, UpdateOffDayDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> DeleteOffDayAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<List<OffDayDto>>> SaveOffDayMatrixAsync(SaveOffDayMatrixDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<List<OffDayDto>>> GetOffDayMatrixAsync(int organizationId, int academicYearId, int? roleId = null, int? locationId = null, bool includeWorkingDays = true, CancellationToken cancellationToken = default);
}

public interface ILeaveTypeService
{
    Task<ApiResponseDto<List<LeaveTypeDto>>> GetLeaveTypesAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<LeaveTypeDto>> GetLeaveTypeByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<LeaveTypeDto>> CreateLeaveTypeAsync(CreateLeaveTypeDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> UpdateLeaveTypeAsync(int id, UpdateLeaveTypeDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> DeleteLeaveTypeAsync(int id, CancellationToken cancellationToken = default);
}

public interface INotificationService
{
    Task<ApiResponseDto<List<NotificationDto>>> GetNotificationsAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default);
}
