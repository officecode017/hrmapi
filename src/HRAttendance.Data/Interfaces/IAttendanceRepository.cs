using HRAttendance.Data.Models.Attendance;

namespace HRAttendance.Data.Interfaces;

public interface IAttendanceRepository
{
    Task<EmployeeAttendance?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<EmployeeAttendance?> GetTodayAttendanceAsync(int employeeId, DateOnly date, CancellationToken cancellationToken = default);
    Task<List<EmployeeAttendance>> GetHistoryAsync(int employeeId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);
    Task<EmployeeAttendance> AddAsync(EmployeeAttendance attendance, CancellationToken cancellationToken = default);
    Task UpdateAsync(EmployeeAttendance attendance, CancellationToken cancellationToken = default);
}
