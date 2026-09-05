using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Leave;

namespace HRAttendance.Business.Interfaces;

public interface ILeaveService
{
    Task<ApiResponseDto<LeaveApplicationDto>> ApplyLeaveAsync(CreateLeaveApplicationDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<List<LeaveApplicationDto>>> GetEmployeeLeavesAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<List<LeaveApplicationDto>>> GetPendingLeavesAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> ProcessLeaveApprovalAsync(int leaveApplicationId, int approverId, LeaveApprovalDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> CancelLeaveAsync(int leaveApplicationId, int employeeId, bool isAdmin = false, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<List<LeaveBalanceDto>>> GetLeaveBalancesAsync(int employeeId, int academicYearId, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<bool>> AdjustLeaveBalanceAsync(AdjustLeaveBalanceDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<List<EmployeeLeaveBalanceDetailDto>>> GetAllLeaveBalancesAsync(int organizationId, int? academicYearId, int? departmentId, int? leaveTypeId, string? search, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<int>> BulkAllocateLeaveBalancesAsync(BulkAllocateLeaveBalanceDto request, CancellationToken cancellationToken = default);
}

