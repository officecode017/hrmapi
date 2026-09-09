using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Interfaces.Payroll;

public record CreateAdjustmentDto(
    int OrganizationId,
    int EmployeeId,
    int PayrollPeriodId,
    AdjustmentType Type,
    AdjustmentDirection Direction,
    decimal Amount,
    string Reason
);

public interface IPayrollAdjustmentService
{
    Task<PayrollAdjustment> CreateAdjustmentAsync(CreateAdjustmentDto dto, int userId, CancellationToken cancellationToken = default);
    Task<PayrollAdjustment> ApproveAdjustmentAsync(int adjustmentId, int approvedByUserId, CancellationToken cancellationToken = default);
    Task<PayrollAdjustment> RejectAdjustmentAsync(int adjustmentId, int rejectedByUserId, string rejectionReason, CancellationToken cancellationToken = default);
    Task<PayrollAdjustment> CancelAdjustmentAsync(int adjustmentId, int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PayrollAdjustment>> GetAdjustmentsByPeriodAsync(int periodId, CancellationToken cancellationToken = default);
}
