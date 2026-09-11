using Microsoft.EntityFrameworkCore;
using HRAttendance.Business.Interfaces.Payroll;
using HRAttendance.Data;
using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Services.Payroll;

public class PayrollAdjustmentService : IPayrollAdjustmentService
{
    private readonly ApplicationDbContext _context;

    public PayrollAdjustmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PayrollAdjustment> CreateAdjustmentAsync(CreateAdjustmentDto dto, int userId, CancellationToken cancellationToken = default)
    {
        if (dto.Amount <= 0)
        {
            throw new ArgumentException("Adjustment amount must be greater than zero.", nameof(dto.Amount));
        }

        var period = await _context.PayrollPeriods
            .FirstOrDefaultAsync(p => p.Id == dto.PayrollPeriodId, cancellationToken)
            ?? throw new InvalidOperationException($"Payroll period {dto.PayrollPeriodId} not found.");

        if (period.Status == PayrollPeriodStatus.Locked ||
            period.Status == PayrollPeriodStatus.PaymentProcessing ||
            period.Status == PayrollPeriodStatus.Paid)
        {
            throw new InvalidOperationException($"Cannot create adjustments for a period in '{period.Status}' status.");
        }

        var year = DateTime.UtcNow.Year;
        var count = await _context.PayrollAdjustments
            .CountAsync(a => a.OrganizationId == dto.OrganizationId, cancellationToken);
        var adjustmentNumber = $"ADJ-{year}-{(count + 1):D6}";

        var adjustment = new PayrollAdjustment
        {
            AdjustmentNumber = adjustmentNumber,
            OrganizationId = dto.OrganizationId,
            EmployeeId = dto.EmployeeId,
            PayrollPeriodId = dto.PayrollPeriodId,
            Type = dto.Type,
            Direction = dto.Direction,
            Amount = dto.Amount,
            Reason = dto.Reason,
            Status = AdjustmentStatus.Pending,
            CreatedBy = userId
        };

        _context.PayrollAdjustments.Add(adjustment);
        await _context.SaveChangesAsync(cancellationToken);

        return adjustment;
    }

    public async Task<PayrollAdjustment> ApproveAdjustmentAsync(int adjustmentId, int approvedByUserId, CancellationToken cancellationToken = default)
    {
        var adj = await _context.PayrollAdjustments
            .FirstOrDefaultAsync(a => a.Id == adjustmentId, cancellationToken)
            ?? throw new InvalidOperationException($"Adjustment {adjustmentId} not found.");

        if (adj.Status != AdjustmentStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot approve adjustment in '{adj.Status}' status.");
        }

        adj.Status = AdjustmentStatus.Approved;
        adj.ApprovedBy = approvedByUserId;
        adj.ApprovedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return adj;
    }

    public async Task<PayrollAdjustment> RejectAdjustmentAsync(int adjustmentId, int rejectedByUserId, string rejectionReason, CancellationToken cancellationToken = default)
    {
        var adj = await _context.PayrollAdjustments
            .FirstOrDefaultAsync(a => a.Id == adjustmentId, cancellationToken)
            ?? throw new InvalidOperationException($"Adjustment {adjustmentId} not found.");

        if (adj.Status != AdjustmentStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot reject adjustment in '{adj.Status}' status.");
        }

        adj.Status = AdjustmentStatus.Rejected;
        adj.RejectedBy = rejectedByUserId;
        adj.RejectedAt = DateTimeOffset.UtcNow;
        adj.RejectionReason = rejectionReason;

        await _context.SaveChangesAsync(cancellationToken);
        return adj;
    }

    public async Task<PayrollAdjustment> CancelAdjustmentAsync(int adjustmentId, int userId, CancellationToken cancellationToken = default)
    {
        var adj = await _context.PayrollAdjustments
            .FirstOrDefaultAsync(a => a.Id == adjustmentId, cancellationToken)
            ?? throw new InvalidOperationException($"Adjustment {adjustmentId} not found.");

        if (adj.Status == AdjustmentStatus.Applied)
        {
            throw new InvalidOperationException("Cannot cancel an already applied adjustment.");
        }

        adj.Status = AdjustmentStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);
        return adj;
    }

    public async Task<IReadOnlyList<PayrollAdjustment>> GetAdjustmentsByPeriodAsync(int periodId, CancellationToken cancellationToken = default)
    {
        return await _context.PayrollAdjustments
            .Include(a => a.Employee)
            .Where(a => a.PayrollPeriodId == periodId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
