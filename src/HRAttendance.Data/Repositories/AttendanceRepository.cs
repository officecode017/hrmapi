using Microsoft.EntityFrameworkCore;
using HRAttendance.Data.Interfaces;
using HRAttendance.Data.Models.Attendance;

namespace HRAttendance.Data.Repositories;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly ApplicationDbContext _context;

    public AttendanceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeAttendance?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.EmployeeAttendances
            .Include(a => a.Shift)
            .Include(a => a.Location)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<EmployeeAttendance?> GetTodayAttendanceAsync(int employeeId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var startOfDay = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var endOfDay = new DateTimeOffset(date.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        return await _context.EmployeeAttendances
            .Include(a => a.Shift)
            .Include(a => a.Location)
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId 
                                   && a.InTime >= startOfDay 
                                   && a.InTime <= endOfDay, 
                                 cancellationToken);
    }

    public async Task<List<EmployeeAttendance>> GetHistoryAsync(int employeeId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default)
    {
        var startRange = new DateTimeOffset(fromDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var endRange = new DateTimeOffset(toDate.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        return await _context.EmployeeAttendances
            .AsNoTracking()
            .Where(a => a.EmployeeId == employeeId && a.InTime >= startRange && a.InTime <= endRange)
            .Include(a => a.Shift)
            .Include(a => a.Location)
            .Include(a => a.Employee)
            .OrderByDescending(a => a.InTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmployeeAttendance> AddAsync(EmployeeAttendance attendance, CancellationToken cancellationToken = default)
    {
        await _context.EmployeeAttendances.AddAsync(attendance, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return attendance;
    }

    public async Task UpdateAsync(EmployeeAttendance attendance, CancellationToken cancellationToken = default)
    {
        _context.EmployeeAttendances.Update(attendance);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
