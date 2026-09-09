using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;

namespace HRAttendance.Data.Models.Payroll;

public class PayrollPayment : AuditableEntity
{
    public int PayrollEmployeeId { get; set; }
    public int OrganizationId { get; set; }
    public int PaymentAttemptNumber { get; set; } = 1;
    public string IdempotencyKey { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public PaymentMethod Method { get; set; } = PaymentMethod.BankTransfer;

    public string? TransactionReference { get; set; }
    public string? BankName { get; set; }
    public string? MaskedAccountNumber { get; set; }
    public string? IFSCCode { get; set; }

    public DateTimeOffset? InitiatedAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public DateTimeOffset? ReversedAt { get; set; }
    public string? FailureReason { get; set; }

    public virtual PayrollEmployee PayrollEmployee { get; set; } = null!;
    public virtual Organization.Organization Organization { get; set; } = null!;
}
