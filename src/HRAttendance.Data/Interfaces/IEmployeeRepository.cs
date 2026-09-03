using HRAttendance.Data.Models.Employee;

namespace HRAttendance.Data.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Employee?> GetByCodeAsync(int organizationId, string employeeCode, CancellationToken cancellationToken = default);
    Task<Employee?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Employee>> GetAllAsync(int organizationId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
    Task<int> CountAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<Employee> AddAsync(Employee employee, CancellationToken cancellationToken = default);
    Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default);
    Task DeleteAsync(Employee employee, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int organizationId, string employeeCode, int? excludeId = null, CancellationToken cancellationToken = default);
}
