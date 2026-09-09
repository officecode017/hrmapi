using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Data.DTOs.Payroll;

public class DisbursePeriodDto
{
    public string IdempotencyKey { get; set; } = string.Empty;
    public PaymentMethod Method { get; set; } = PaymentMethod.BankTransfer;
}

public class RecordPaymentResultRequestDto
{
    public bool IsSuccess { get; set; }
    public string? TransactionReference { get; set; }
    public string? FailureReason { get; set; }
}

public class RetryPaymentDto
{
    public string IdempotencyKey { get; set; } = string.Empty;
}

public class ManualConfirmPaymentRequestDto
{
    public string TransactionReference { get; set; } = string.Empty;
    public DateTimeOffset? PaymentDate { get; set; }
    public PaymentMethod Method { get; set; } = PaymentMethod.BankTransfer;
    public string? Remarks { get; set; }
}

public class ManualOverridePaymentRequestDto
{
    public string Reason { get; set; } = string.Empty;
    public string TransactionReference { get; set; } = string.Empty;
    public DateTimeOffset? PaymentDate { get; set; }
    public PaymentMethod Method { get; set; } = PaymentMethod.BankTransfer;
    public string? Remarks { get; set; }
}

public class PayrollPaymentDto
{
    public int Id { get; set; }
    public int PayrollEmployeeId { get; set; }
    public int OrganizationId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public int PaymentAttemptNumber { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public PaymentMethod Method { get; set; }
    public string? TransactionReference { get; set; }
    public string? BankName { get; set; }
    public string? MaskedAccountNumber { get; set; }
    public string? IFSCCode { get; set; }
    public DateTimeOffset? InitiatedAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public DateTimeOffset? ReversedAt { get; set; }
    public string? FailureReason { get; set; }

    // Manual confirmation and override audit fields
    public bool IsManualConfirmation { get; set; }
    public bool IsManualOverride { get; set; }
    public string? ManualOverrideReason { get; set; }
    public string? Remarks { get; set; }
    public int? ConfirmedBy { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }

    public static PayrollPaymentDto FromEntity(PayrollPayment p)
    {
        var isManualConfirm = p.FailureReason?.StartsWith("[MANUAL CONFIRMATION]") == true;
        var isManualOverride = p.FailureReason?.StartsWith("[MANUAL OVERRIDE]") == true;
        string? overrideReason = null;
        string? remarks = null;

        if (isManualOverride && p.FailureReason != null)
        {
            var parts = p.FailureReason.Split('|');
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (trimmed.StartsWith("[MANUAL OVERRIDE] Reason:", StringComparison.OrdinalIgnoreCase))
                {
                    overrideReason = trimmed.Substring("[MANUAL OVERRIDE] Reason:".Length).Trim();
                }
                else if (trimmed.StartsWith("Remarks:", StringComparison.OrdinalIgnoreCase))
                {
                    remarks = trimmed.Substring("Remarks:".Length).Trim();
                }
            }
        }
        else if (isManualConfirm && p.FailureReason != null)
        {
            var parts = p.FailureReason.Split('|');
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (trimmed.StartsWith("Remarks:", StringComparison.OrdinalIgnoreCase))
                {
                    remarks = trimmed.Substring("Remarks:".Length).Trim();
                }
            }
        }

        return new PayrollPaymentDto
        {
            Id = p.Id,
            PayrollEmployeeId = p.PayrollEmployeeId,
            OrganizationId = p.OrganizationId,
            EmployeeId = p.PayrollEmployee?.EmployeeId ?? 0,
            EmployeeName = p.PayrollEmployee?.Employee != null
                ? $"{p.PayrollEmployee.Employee.FirstName} {p.PayrollEmployee.Employee.LastName}".Trim()
                : string.Empty,
            EmployeeCode = p.PayrollEmployee?.Employee?.EmployeeCode ?? string.Empty,
            PaymentAttemptNumber = p.PaymentAttemptNumber,
            IdempotencyKey = p.IdempotencyKey,
            Amount = p.Amount,
            Status = p.Status,
            Method = p.Method,
            TransactionReference = p.TransactionReference,
            BankName = p.BankName,
            MaskedAccountNumber = p.MaskedAccountNumber,
            IFSCCode = p.IFSCCode,
            InitiatedAt = p.InitiatedAt,
            ProcessedAt = p.ProcessedAt,
            PaidAt = p.PaidAt,
            ReversedAt = p.ReversedAt,
            FailureReason = p.FailureReason,
            IsManualConfirmation = isManualConfirm || isManualOverride,
            IsManualOverride = isManualOverride,
            ManualOverrideReason = overrideReason,
            Remarks = remarks,
            ConfirmedBy = p.ModifiedBy,
            ConfirmedAt = p.ModifiedAt.HasValue ? new DateTimeOffset(p.ModifiedAt.Value, TimeSpan.Zero) : p.PaidAt
        };
    }
}
