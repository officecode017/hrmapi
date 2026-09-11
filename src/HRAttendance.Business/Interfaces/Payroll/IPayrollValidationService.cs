using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Interfaces.Payroll;

public record ValidationSummary(
    int TotalExceptions,
    int CriticalCount,
    int WarningCount,
    int InfoCount,
    bool CanApproveAndLock,
    IReadOnlyList<PayrollException> Exceptions
);

public interface IPayrollValidationService
{
    Task<ValidationSummary> ValidatePeriodAsync(int payrollPeriodId, CancellationToken cancellationToken = default);
    Task<PayrollException> ResolveExceptionAsync(int exceptionId, int resolvedByUserId, string notes, CancellationToken cancellationToken = default);
}
