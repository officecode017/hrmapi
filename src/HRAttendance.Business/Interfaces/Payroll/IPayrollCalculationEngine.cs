using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Interfaces.Payroll;

public record PayrollCalculationBatchResult(
    int PayrollPeriodId,
    int TotalProcessed,
    int TotalSucceeded,
    int TotalExceptions,
    decimal TotalGrossPay,
    decimal TotalDeductions,
    decimal TotalNetPay,
    string PolicySnapshotJson
);

public interface IPayrollCalculationEngine
{
    Task<PayrollEmployee> CalculateEmployeePayrollAsync(int payrollPeriodId, int employeeId, CancellationToken cancellationToken = default);
    Task<PayrollCalculationBatchResult> CalculatePeriodBatchAsync(int payrollPeriodId, CancellationToken cancellationToken = default);
}
