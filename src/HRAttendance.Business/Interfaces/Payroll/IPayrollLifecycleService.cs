using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Interfaces.Payroll;

public interface IPayrollLifecycleService
{
    Task<PayrollPeriod> SubmitForReviewAsync(int periodId, int userId, CancellationToken cancellationToken = default);
    Task<PayrollPeriod> ApproveAsync(int periodId, int userId, CancellationToken cancellationToken = default);
    Task<PayrollPeriod> LockAsync(int periodId, int userId, CancellationToken cancellationToken = default);
    Task<PayrollPeriod> ReopenAsync(int periodId, int userId, CancellationToken cancellationToken = default);
    Task<PayrollPeriod> CancelAsync(int periodId, int userId, CancellationToken cancellationToken = default);
    Task<PayrollPeriod> ResetFinancialsAsync(int periodId, int userId, CancellationToken cancellationToken = default);
    Task DeletePeriodAsync(int periodId, int userId, CancellationToken cancellationToken = default);
}
