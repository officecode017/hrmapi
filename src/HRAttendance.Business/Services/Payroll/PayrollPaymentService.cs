using Microsoft.EntityFrameworkCore;
using HRAttendance.Business.Interfaces.Payroll;
using HRAttendance.Data;
using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Services.Payroll;

public class PayrollPaymentService : IPayrollPaymentService
{
    private readonly ApplicationDbContext _context;

    public PayrollPaymentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DisbursementBatchResult> InitiateDisbursementAsync(int periodId, string idempotencyKey, int userId, CancellationToken cancellationToken = default)
    {
        var period = await _context.PayrollPeriods
            .Include(p => p.Employees)
            .FirstOrDefaultAsync(p => p.Id == periodId, cancellationToken)
            ?? throw new InvalidOperationException($"Payroll period {periodId} not found.");

        // Check for existing payments under this idempotency prefix first (idempotent replay)
        var existingPayments = await _context.PayrollPayments
            .Where(p => p.IdempotencyKey.StartsWith(idempotencyKey))
            .ToListAsync(cancellationToken);

        if (existingPayments.Count > 0)
        {
            return new DisbursementBatchResult(
                PayrollPeriodId: periodId,
                IdempotencyKey: idempotencyKey,
                TotalPaymentsInitiated: existingPayments.Count,
                TotalAmountDisbursed: existingPayments.Sum(p => p.Amount),
                Payments: existingPayments
            );
        }

        if (period.Status != PayrollPeriodStatus.Locked && period.Status != PayrollPeriodStatus.Approved)
        {
            throw new InvalidOperationException($"Payroll period must be Approved or Locked before initiating disbursement. Current status: '{period.Status}'.");
        }

        var eligibleEmployees = await _context.PayrollEmployees
            .Include(pe => pe.Employee)
                .ThenInclude(e => e.ContactDetails)
            .Where(pe => pe.PayrollPeriodId == periodId && pe.NetPay > 0)
            .ToListAsync(cancellationToken);

        var payments = new List<PayrollPayment>();

        foreach (var pe in eligibleEmployees)
        {
            var payment = new PayrollPayment
            {
                PayrollEmployeeId = pe.Id,
                OrganizationId = pe.OrganizationId,
                PaymentAttemptNumber = 1,
                IdempotencyKey = $"{idempotencyKey}-{pe.Id}",
                Amount = pe.NetPay,
                Status = PaymentStatus.Processing,
                Method = PaymentMethod.BankTransfer,
                BankName = "HDFC Bank",
                MaskedAccountNumber = "XXXX-XXXX-" + (pe.EmployeeId % 10000).ToString("D4"),
                IFSCCode = "HDFC0001234",
                InitiatedAt = DateTimeOffset.UtcNow,
                CreatedBy = userId
            };

            payments.Add(payment);
            _context.PayrollPayments.Add(payment);
        }

        period.Status = PayrollPeriodStatus.PaymentProcessing;
        await _context.SaveChangesAsync(cancellationToken);

        return new DisbursementBatchResult(
            PayrollPeriodId: periodId,
            IdempotencyKey: idempotencyKey,
            TotalPaymentsInitiated: payments.Count,
            TotalAmountDisbursed: payments.Sum(p => p.Amount),
            Payments: payments
        );
    }

    public async Task<PayrollPayment> RecordPaymentResultAsync(RecordPaymentResultDto dto, int userId, CancellationToken cancellationToken = default)
    {
        var payment = await _context.PayrollPayments
            .Include(p => p.PayrollEmployee)
                .ThenInclude(pe => pe.Period)
            .FirstOrDefaultAsync(p => p.Id == dto.PaymentId, cancellationToken)
            ?? throw new InvalidOperationException($"Payment {dto.PaymentId} not found.");

        if (dto.Success)
        {
            payment.Status = PaymentStatus.Paid;
            payment.PaidAt = DateTimeOffset.UtcNow;
            payment.ProcessedAt = DateTimeOffset.UtcNow;
            payment.TransactionReference = dto.TransactionReference ?? $"TXN-{Guid.NewGuid():N}";
        }
        else
        {
            payment.Status = PaymentStatus.Failed;
            payment.ProcessedAt = DateTimeOffset.UtcNow;
            payment.FailureReason = dto.FailureReason ?? "Disbursement rejected by payment gateway/bank.";
        }

        payment.ModifiedBy = userId;
        payment.ModifiedAt = DateTime.UtcNow;

        await CheckAndUpdatePeriodPaidStatusAsync(payment.PayrollEmployee.PayrollPeriodId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return payment;
    }

    public async Task<PayrollPayment> ConfirmPaymentManuallyAsync(ManualConfirmPaymentDto dto, int userId, CancellationToken cancellationToken = default)
    {
        var payment = await _context.PayrollPayments
            .Include(p => p.PayrollEmployee)
                .ThenInclude(pe => pe.Period)
            .FirstOrDefaultAsync(p => p.Id == dto.PaymentId, cancellationToken)
            ?? throw new InvalidOperationException($"Payment {dto.PaymentId} not found.");

        if (payment.Status == PaymentStatus.Paid)
        {
            throw new InvalidOperationException($"Payment {dto.PaymentId} is already marked as Paid.");
        }

        if (payment.Status == PaymentStatus.Reversed)
        {
            throw new InvalidOperationException($"Payment {dto.PaymentId} has been reversed and cannot be confirmed.");
        }

        payment.Status = PaymentStatus.Paid;
        payment.TransactionReference = dto.TransactionReference;
        payment.Method = dto.Method;
        payment.PaidAt = dto.PaymentDate;
        payment.ProcessedAt = DateTimeOffset.UtcNow;
        payment.ModifiedBy = userId;
        payment.ModifiedAt = DateTime.UtcNow;
        payment.FailureReason = $"[MANUAL CONFIRMATION] Method: {dto.Method} | Ref: {dto.TransactionReference}{(string.IsNullOrWhiteSpace(dto.Remarks) ? "" : $" | Remarks: {dto.Remarks}")} | ConfirmedBy: {userId} at {DateTimeOffset.UtcNow:u}";

        await CheckAndUpdatePeriodPaidStatusAsync(payment.PayrollEmployee.PayrollPeriodId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return payment;
    }

    public async Task<PayrollPayment> OverridePaymentAsync(ManualOverridePaymentDto dto, int userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Reason))
        {
            throw new InvalidOperationException("Override reason is mandatory for manual payment override.");
        }

        var payment = await _context.PayrollPayments
            .Include(p => p.PayrollEmployee)
                .ThenInclude(pe => pe.Period)
            .FirstOrDefaultAsync(p => p.Id == dto.PaymentId, cancellationToken)
            ?? throw new InvalidOperationException($"Payment {dto.PaymentId} not found.");

        payment.Status = PaymentStatus.Paid;
        payment.TransactionReference = string.IsNullOrWhiteSpace(dto.TransactionReference) ? $"OVERRIDE-{Guid.NewGuid():N}" : dto.TransactionReference;
        payment.Method = dto.Method;
        payment.PaidAt = dto.PaymentDate;
        payment.ProcessedAt = DateTimeOffset.UtcNow;
        payment.ModifiedBy = userId;
        payment.ModifiedAt = DateTime.UtcNow;
        payment.FailureReason = $"[MANUAL OVERRIDE] Reason: {dto.Reason} | Method: {dto.Method} | Ref: {payment.TransactionReference}{(string.IsNullOrWhiteSpace(dto.Remarks) ? "" : $" | Remarks: {dto.Remarks}")} | OverriddenBy: {userId} at {DateTimeOffset.UtcNow:u}";

        await CheckAndUpdatePeriodPaidStatusAsync(payment.PayrollEmployee.PayrollPeriodId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return payment;
    }

    private async Task CheckAndUpdatePeriodPaidStatusAsync(int periodId, CancellationToken cancellationToken)
    {
        var eligibleEmployeeCount = await _context.PayrollEmployees
            .CountAsync(pe => pe.PayrollPeriodId == periodId && pe.NetPay > 0, cancellationToken);

        if (eligibleEmployeeCount == 0)
        {
            return;
        }

        var periodPayments = await _context.PayrollPayments
            .Where(p => p.PayrollEmployee.PayrollPeriodId == periodId)
            .ToListAsync(cancellationToken);

        // Evaluate the latest attempt for each employee
        var latestPayments = periodPayments
            .GroupBy(p => p.PayrollEmployeeId)
            .Select(g => g.OrderByDescending(p => p.PaymentAttemptNumber).First())
            .ToList();

        // The payroll period transitions from PaymentProcessing to Paid only when every eligible payment is in Paid status
        if (latestPayments.Count == eligibleEmployeeCount && latestPayments.All(p => p.Status == PaymentStatus.Paid))
        {
            var period = await _context.PayrollPeriods.FindAsync(new object[] { periodId }, cancellationToken);
            if (period != null && period.Status == PayrollPeriodStatus.PaymentProcessing)
            {
                period.Status = PayrollPeriodStatus.Paid;
            }
        }
    }

    public async Task<PayrollPayment> RetryPaymentAsync(int paymentId, string idempotencyKey, int userId, CancellationToken cancellationToken = default)
    {
        var failedPayment = await _context.PayrollPayments
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken)
            ?? throw new InvalidOperationException($"Payment {paymentId} not found.");

        if (failedPayment.Status != PaymentStatus.Failed)
        {
            throw new InvalidOperationException($"Only failed payments can be retried. Current status: '{failedPayment.Status}'.");
        }

        var existingRetry = await _context.PayrollPayments
            .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey, cancellationToken);

        if (existingRetry != null)
        {
            return existingRetry;
        }

        var newAttempt = new PayrollPayment
        {
            PayrollEmployeeId = failedPayment.PayrollEmployeeId,
            OrganizationId = failedPayment.OrganizationId,
            PaymentAttemptNumber = failedPayment.PaymentAttemptNumber + 1,
            IdempotencyKey = idempotencyKey,
            Amount = failedPayment.Amount,
            Status = PaymentStatus.Processing,
            Method = failedPayment.Method,
            BankName = failedPayment.BankName,
            MaskedAccountNumber = failedPayment.MaskedAccountNumber,
            IFSCCode = failedPayment.IFSCCode,
            InitiatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId
        };

        _context.PayrollPayments.Add(newAttempt);
        await _context.SaveChangesAsync(cancellationToken);

        return newAttempt;
    }

    public async Task<PayrollPayment> ReversePaymentAsync(int paymentId, string reason, int userId, CancellationToken cancellationToken = default)
    {
        var payment = await _context.PayrollPayments
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken)
            ?? throw new InvalidOperationException($"Payment {paymentId} not found.");

        if (payment.Status != PaymentStatus.Paid)
        {
            throw new InvalidOperationException($"Only paid payments can be reversed. Current status: '{payment.Status}'.");
        }

        payment.Status = PaymentStatus.Reversed;
        payment.ReversedAt = DateTimeOffset.UtcNow;
        payment.FailureReason = $"Reversed: {reason}";

        await _context.SaveChangesAsync(cancellationToken);
        return payment;
    }

    public async Task<IReadOnlyList<PayrollPayment>> GetPaymentsByPeriodAsync(int periodId, CancellationToken cancellationToken = default)
    {
        return await _context.PayrollPayments
            .Include(p => p.PayrollEmployee)
                .ThenInclude(pe => pe.Employee)
            .Where(p => p.PayrollEmployee.PayrollPeriodId == periodId)
            .OrderByDescending(p => p.InitiatedAt)
            .ToListAsync(cancellationToken);
    }
}
