using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Interfaces.Payroll;

public record CreateArrearDto(
    int OrganizationId,
    int EmployeeId,
    int SourcePayrollPeriodId,
    int TargetPayrollPeriodId,
    string ComponentCode,
    decimal OriginalAmount,
    decimal CorrectAmount,
    string Reason
);

public interface IPayrollArrearService
{
    Task<PayrollArrear> CreateArrearAsync(CreateArrearDto dto, int userId, CancellationToken cancellationToken = default);
    Task<PayrollArrear> CalculateArrearDifferenceAsync(int arrearId, CancellationToken cancellationToken = default);
    Task<PayrollArrear> ApplyArrearAsync(int arrearId, int targetPeriodId, CancellationToken cancellationToken = default);
    Task<PayrollArrear> CancelArrearAsync(int arrearId, int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PayrollArrear>> GetArrearsByTargetPeriodAsync(int targetPeriodId, CancellationToken cancellationToken = default);
}
