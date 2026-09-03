using Microsoft.EntityFrameworkCore;
using HRAttendance.Data.Interfaces;
using HRAttendance.Data.Models.Leave;

namespace HRAttendance.Data.Repositories;

public class LeaveRepository : ILeaveRepository
{
    private readonly ApplicationDbContext _context;

    public LeaveRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LeaveApplication?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.LeaveApplications
            .Include(l => l.LeaveType)
            .Include(l => l.Employee)
            .Include(l => l.SubLeaveApplications)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<List<LeaveApplication>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.LeaveApplications
            .AsNoTracking()
            .Where(l => l.EmployeeId == employeeId)
            .Include(l => l.LeaveType)
            .OrderByDescending(l => l.LeaveApplicationDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LeaveApplication>> GetPendingByOrganizationAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        return await _context.LeaveApplications
            .AsNoTracking()
            .Where(l => l.OrganizationId == organizationId && l.Status == "Pending")
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .OrderBy(l => l.LeaveApplicationDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmployeeLeave?> GetEmployeeLeaveBalanceAsync(int employeeId, int leaveTypeId, int academicYearId, CancellationToken cancellationToken = default)
    {
        return await _context.EmployeeLeaves
            .FirstOrDefaultAsync(el => el.EmployeeId == employeeId 
                                    && el.LeaveTypeId == leaveTypeId 
                                    && el.AcademicYearId == academicYearId, 
                                 cancellationToken);
    }

    public async Task<List<EmployeeLeave>> GetEmployeeLeaveBalancesAsync(int employeeId, int academicYearId, CancellationToken cancellationToken = default)
    {
        return await _context.EmployeeLeaves
            .AsNoTracking()
            .Where(el => el.EmployeeId == employeeId && el.AcademicYearId == academicYearId)
            .Include(el => el.LeaveType)
            .ToListAsync(cancellationToken);
    }

    public async Task<LeaveApplication> AddApplicationAsync(LeaveApplication application, CancellationToken cancellationToken = default)
    {
        await _context.LeaveApplications.AddAsync(application, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return application;
    }

    public async Task UpdateApplicationAsync(LeaveApplication application, CancellationToken cancellationToken = default)
    {
        _context.LeaveApplications.Update(application);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateLeaveBalanceAsync(EmployeeLeave leaveBalance, CancellationToken cancellationToken = default)
    {
        _context.EmployeeLeaves.Update(leaveBalance);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddSubApplicationsAsync(IEnumerable<SubLeaveApplication> subApplications, CancellationToken cancellationToken = default)
    {
        await _context.SubLeaveApplications.AddRangeAsync(subApplications, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
