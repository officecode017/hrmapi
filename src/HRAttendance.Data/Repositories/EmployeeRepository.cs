using Microsoft.EntityFrameworkCore;
using HRAttendance.Data.Interfaces;
using HRAttendance.Data.Models.Employee;

namespace HRAttendance.Data.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Employee?> GetByCodeAsync(int organizationId, string employeeCode, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Include(e => e.EmployeeRoles)
                .ThenInclude(er => er.Role)
            .Include(e => e.ContactDetails)
            .Include(e => e.ProfessionalDetails)
            .FirstOrDefaultAsync(e => e.OrganizationId == organizationId && e.EmployeeCode == employeeCode, cancellationToken);
    }

    public async Task<Employee?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Include(e => e.ContactDetails)
            .Include(e => e.ProfessionalDetails)
                .ThenInclude(p => p!.Department)
            .Include(e => e.ProfessionalDetails)
                .ThenInclude(p => p!.Designation)
            .Include(e => e.ProfessionalDetails)
                .ThenInclude(p => p!.Location)
            .Include(e => e.ProfessionalDetails)
                .ThenInclude(p => p!.Shift)
            .Include(e => e.ProfessionalDetails)
                .ThenInclude(p => p!.Manager)
            .Include(e => e.EmployeeRoles)
                .ThenInclude(er => er.Role)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<List<Employee>> GetAllAsync(int organizationId, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .AsNoTracking()
            .Where(e => e.OrganizationId == organizationId)
            .Include(e => e.ContactDetails)
            .Include(e => e.ProfessionalDetails)
                .ThenInclude(p => p!.Department)
            .Include(e => e.ProfessionalDetails)
                .ThenInclude(p => p!.Designation)
            .OrderBy(e => e.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Where(e => e.OrganizationId == organizationId)
            .CountAsync(cancellationToken);
    }

    public async Task<Employee> AddAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        await _context.Employees.AddAsync(employee, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return employee;
    }

    public async Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        employee.IsDeleted = true;
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int organizationId, string employeeCode, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .AnyAsync(e => e.OrganizationId == organizationId 
                        && e.EmployeeCode == employeeCode 
                        && (!excludeId.HasValue || e.Id != excludeId.Value), 
                      cancellationToken);
    }
}
