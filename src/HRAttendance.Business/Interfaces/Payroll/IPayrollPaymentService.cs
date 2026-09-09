using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Interfaces.Payroll;

public record RecordPaymentResultDto(
    int PaymentId,
    bool Success,
    string? TransactionReference,
    string? FailureReason
);

public record ManualConfirmPaymentDto(
    int PaymentId,
    string TransactionReference,
    DateTimeOffset PaymentDate,
    PaymentMethod Method,
    string? Remarks
);

public record ManualOverridePaymentDto(
    int PaymentId,
    string Reason,
    string TransactionReference,
    DateTimeOffset PaymentDate,
    PaymentMethod Method,
    string? Remarks
);

public record DisbursementBatchResult(
    int PayrollPeriodId,
    string IdempotencyKey,
    int TotalPaymentsInitiated,
    decimal TotalAmountDisbursed,
    IReadOnlyList<PayrollPayment> Payments
);

public interface IPayrollPaymentService
{
    Task<DisbursementBatchResult> InitiateDisbursementAsync(int periodId, string idempotencyKey, int userId, CancellationToken cancellationToken = default);
    Task<PayrollPayment> RecordPaymentResultAsync(RecordPaymentResultDto dto, int userId, CancellationToken cancellationToken = default);
    Task<PayrollPayment> ConfirmPaymentManuallyAsync(ManualConfirmPaymentDto dto, int userId, CancellationToken cancellationToken = default);
    Task<PayrollPayment> OverridePaymentAsync(ManualOverridePaymentDto dto, int userId, CancellationToken cancellationToken = default);
    Task<PayrollPayment> RetryPaymentAsync(int paymentId, string idempotencyKey, int userId, CancellationToken cancellationToken = default);
    Task<PayrollPayment> ReversePaymentAsync(int paymentId, string reason, int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PayrollPayment>> GetPaymentsByPeriodAsync(int periodId, CancellationToken cancellationToken = default);
}
