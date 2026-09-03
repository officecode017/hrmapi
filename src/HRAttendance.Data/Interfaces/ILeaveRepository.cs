using HRAttendance.Data.Models.Leave;

namespace HRAttendance.Data.Interfaces;

public interface ILeaveRepository
{
    Task<LeaveApplication?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<LeaveApplication>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<List<LeaveApplication>> GetPendingByOrganizationAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<EmployeeLeave?> GetEmployeeLeaveBalanceAsync(int employeeId, int leaveTypeId, int academicYearId, CancellationToken cancellationToken = default);
    Task<List<EmployeeLeave>> GetEmployeeLeaveBalancesAsync(int employeeId, int academicYearId, CancellationToken cancellationToken = default);
    Task<LeaveApplication> AddApplicationAsync(LeaveApplication application, CancellationToken cancellationToken = default);
    Task UpdateApplicationAsync(LeaveApplication application, CancellationToken cancellationToken = default);
    Task UpdateLeaveBalanceAsync(EmployeeLeave leaveBalance, CancellationToken cancellationToken = default);
    Task AddSubApplicationsAsync(IEnumerable<SubLeaveApplication> subApplications, CancellationToken cancellationToken = default);
}
