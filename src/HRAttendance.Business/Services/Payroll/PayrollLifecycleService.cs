using Microsoft.EntityFrameworkCore;
using HRAttendance.Business.Interfaces.Payroll;
using HRAttendance.Data;
using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Services.Payroll;

public class PayrollLifecycleService : IPayrollLifecycleService
{
    private readonly ApplicationDbContext _context;
    private readonly IPayrollValidationService _validationService;
    private readonly IPayslipProjectionService _payslipProjectionService;

    public PayrollLifecycleService(
        ApplicationDbContext context,
        IPayrollValidationService validationService,
        IPayslipProjectionService payslipProjectionService)
    {
        _context = context;
        _validationService = validationService;
        _payslipProjectionService = payslipProjectionService;
    }

    public async Task<PayrollPeriod> SubmitForReviewAsync(int periodId, int userId, CancellationToken cancellationToken = default)
    {
        var period = await _context.PayrollPeriods
            .FirstOrDefaultAsync(p => p.Id == periodId, cancellationToken)
            ?? throw new InvalidOperationException($"Payroll period {periodId} not found.");

        if (period.Status != PayrollPeriodStatus.Calculated && period.Status != PayrollPeriodStatus.Draft)
        {
            throw new InvalidOperationException($"Cannot submit payroll period in '{period.Status}' status for review.");
        }

        var validation = await _validationService.ValidatePeriodAsync(periodId, cancellationToken);
        if (validation.CriticalCount > 0)
        {
            throw new InvalidOperationException($"Cannot submit for review: {validation.CriticalCount} critical exceptions exist. Please resolve them first.");
        }

        period.Status = PayrollPeriodStatus.UnderReview;
        await _context.SaveChangesAsync(cancellationToken);

        return period;
    }

    public async Task<PayrollPeriod> ApproveAsync(int periodId, int userId, CancellationToken cancellationToken = default)
    {
        var period = await _context.PayrollPeriods
            .FirstOrDefaultAsync(p => p.Id == periodId, cancellationToken)
            ?? throw new InvalidOperationException($"Payroll period {periodId} not found.");

        if (period.Status != PayrollPeriodStatus.UnderReview && period.Status != PayrollPeriodStatus.Calculated)
        {
            throw new InvalidOperationException($"Cannot approve payroll period in '{period.Status}' status.");
        }

        var validation = await _validationService.ValidatePeriodAsync(periodId, cancellationToken);
        if (!validation.CanApproveAndLock)
        {
            throw new InvalidOperationException($"Cannot approve payroll period: {validation.CriticalCount} critical exceptions remain unresolved.");
        }

        period.Status = PayrollPeriodStatus.Approved;
        period.ApprovedBy = userId;
        period.ApprovedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return period;
    }

    public async Task<PayrollPeriod> LockAsync(int periodId, int userId, CancellationToken cancellationToken = default)
    {
        var period = await _context.PayrollPeriods
            .FirstOrDefaultAsync(p => p.Id == periodId, cancellationToken)
            ?? throw new InvalidOperationException($"Payroll period {periodId} not found.");

        if (period.Status != PayrollPeriodStatus.Approved)
        {
            throw new InvalidOperationException($"Payroll period must be Approved before it can be Locked. Current status: '{period.Status}'.");
        }

        // 1. Lock period
        period.Status = PayrollPeriodStatus.Locked;
        period.LockedBy = userId;
        period.LockedAt = DateTimeOffset.UtcNow;

        // 2. Generate pure projection payslips
        await _payslipProjectionService.ProjectPayslipsForPeriodAsync(period.Id, cancellationToken);

        // 3. Mark attendance records as processed for payroll
        var attendances = await _context.EmployeeAttendances
            .Where(a => a.OrganizationId == period.OrganizationId
                        && a.InTime != null
                        && DateOnly.FromDateTime(a.InTime.Value.Date) >= period.StartDate
                        && DateOnly.FromDateTime(a.InTime.Value.Date) <= period.EndDate
                        && !a.IsProcessedForPayroll)
            .ToListAsync(cancellationToken);

        foreach (var att in attendances)
        {
            att.IsProcessedForPayroll = true;
        }

        // 4. Mark approved adjustments as Applied
        var adjustments = await _context.PayrollAdjustments
            .Where(a => a.PayrollPeriodId == period.Id && a.Status == AdjustmentStatus.Approved)
            .ToListAsync(cancellationToken);

        foreach (var adj in adjustments)
        {
            adj.Status = AdjustmentStatus.Applied;
        }

        // 5. Mark pending/calculated arrears as Applied
        var arrears = await _context.PayrollArrears
            .Where(a => a.TargetPayrollPeriodId == period.Id && (a.Status == ArrearStatus.Pending || a.Status == ArrearStatus.Calculated))
            .ToListAsync(cancellationToken);

        foreach (var arr in arrears)
        {
            arr.Status = ArrearStatus.Applied;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return period;
    }

    public async Task<PayrollPeriod> ReopenAsync(int periodId, int userId, CancellationToken cancellationToken = default)
    {
        var period = await _context.PayrollPeriods
            .FirstOrDefaultAsync(p => p.Id == periodId, cancellationToken)
            ?? throw new InvalidOperationException($"Payroll period {periodId} not found.");

        if (period.Status == PayrollPeriodStatus.PaymentProcessing || period.Status == PayrollPeriodStatus.Paid)
        {
            throw new InvalidOperationException($"Cannot reopen payroll period in '{period.Status}' status once payment processing has begun.");
        }

        // If previously locked, un-stamp attendance records
        if (period.Status == PayrollPeriodStatus.Locked)
        {
            var attendances = await _context.EmployeeAttendances
                .Where(a => a.OrganizationId == period.OrganizationId
                            && a.InTime != null
                            && DateOnly.FromDateTime(a.InTime.Value.Date) >= period.StartDate
                            && DateOnly.FromDateTime(a.InTime.Value.Date) <= period.EndDate
                            && a.IsProcessedForPayroll)
                .ToListAsync(cancellationToken);

            foreach (var att in attendances)
            {
                att.IsProcessedForPayroll = false;
            }

            // Revert applied adjustments and arrears
            var adjustments = await _context.PayrollAdjustments
                .Where(a => a.PayrollPeriodId == period.Id && a.Status == AdjustmentStatus.Applied)
                .ToListAsync(cancellationToken);

            foreach (var adj in adjustments)
            {
                adj.Status = AdjustmentStatus.Approved;
            }

            var arrears = await _context.PayrollArrears
                .Where(a => a.TargetPayrollPeriodId == period.Id && a.Status == ArrearStatus.Applied)
                .ToListAsync(cancellationToken);

            foreach (var arr in arrears)
            {
                arr.Status = ArrearStatus.Calculated;
            }
        }

        period.Status = PayrollPeriodStatus.Draft;
        period.ApprovedBy = null;
        period.ApprovedAt = null;
        period.LockedBy = null;
        period.LockedAt = null;

        await _context.SaveChangesAsync(cancellationToken);
        return period;
    }

    public async Task<PayrollPeriod> CancelAsync(int periodId, int userId, CancellationToken cancellationToken = default)
    {
        var period = await _context.PayrollPeriods
            .FirstOrDefaultAsync(p => p.Id == periodId, cancellationToken)
            ?? throw new InvalidOperationException($"Payroll period {periodId} not found.");

        if (period.Status == PayrollPeriodStatus.Locked ||
            period.Status == PayrollPeriodStatus.PaymentProcessing ||
            period.Status == PayrollPeriodStatus.Paid)
        {
            throw new InvalidOperationException($"Cannot cancel payroll period in '{period.Status}' status.");
        }

        period.Status = PayrollPeriodStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);
        return period;
    }

    public async Task<PayrollPeriod> ResetFinancialsAsync(int periodId, int userId, CancellationToken cancellationToken = default)
    {
        var period = await _context.PayrollPeriods
            .Include(p => p.Employees)
            .FirstOrDefaultAsync(p => p.Id == periodId, cancellationToken)
            ?? throw new InvalidOperationException($"Payroll period {periodId} not found.");

        if (period.Status == PayrollPeriodStatus.PaymentProcessing || period.Status == PayrollPeriodStatus.Paid)
        {
            throw new InvalidOperationException($"Cannot reset financials for period in '{period.Status}' status once payment processing has begun.");
        }

        // 1. Un-stamp attendance records if period was locked
        if (period.Status == PayrollPeriodStatus.Locked)
        {
            var attendances = await _context.EmployeeAttendances
                .Where(a => a.OrganizationId == period.OrganizationId
                            && a.InTime != null
                            && DateOnly.FromDateTime(a.InTime.Value.Date) >= period.StartDate
                            && DateOnly.FromDateTime(a.InTime.Value.Date) <= period.EndDate
                            && a.IsProcessedForPayroll)
                .ToListAsync(cancellationToken);

            foreach (var att in attendances)
            {
                att.IsProcessedForPayroll = false;
            }

            // Revert applied adjustments and arrears
            var adjustments = await _context.PayrollAdjustments
                .Where(a => a.PayrollPeriodId == period.Id && a.Status == AdjustmentStatus.Applied)
                .ToListAsync(cancellationToken);

            foreach (var adj in adjustments)
            {
                adj.Status = AdjustmentStatus.Approved;
            }

            var arrears = await _context.PayrollArrears
                .Where(a => a.TargetPayrollPeriodId == period.Id && a.Status == ArrearStatus.Applied)
                .ToListAsync(cancellationToken);

            foreach (var arr in arrears)
            {
                arr.Status = ArrearStatus.Calculated;
            }
        }

        var peIds = period.Employees.Select(pe => pe.Id).ToList();

        if (peIds.Any())
        {
            // 2. Delete Payslips & PayslipItems
            var payslipIds = await _context.Payslips
                .Where(ps => peIds.Contains(ps.PayrollEmployeeId))
                .Select(ps => ps.Id)
                .ToListAsync(cancellationToken);

            if (payslipIds.Any())
            {
                await _context.PayslipItems
                    .Where(psi => payslipIds.Contains(psi.PayslipId))
                    .ExecuteDeleteAsync(cancellationToken);

                await _context.PayslipAccessLogs
                    .Where(pal => payslipIds.Contains(pal.PayslipId))
                    .ExecuteDeleteAsync(cancellationToken);

                await _context.Payslips
                    .Where(ps => payslipIds.Contains(ps.Id))
                    .ExecuteDeleteAsync(cancellationToken);
            }

            // 3. Delete Slices and Items
            await _context.PayrollSalarySlices
                .Where(s => peIds.Contains(s.PayrollEmployeeId))
                .ExecuteDeleteAsync(cancellationToken);

            await _context.PayrollItems
                .Where(i => peIds.Contains(i.PayrollEmployeeId))
                .ExecuteDeleteAsync(cancellationToken);

            // 4. Delete PayrollEmployees
            await _context.PayrollEmployees
                .Where(pe => pe.PayrollPeriodId == period.Id)
                .ExecuteDeleteAsync(cancellationToken);
        }

        // 5. Delete Exceptions
        await _context.PayrollExceptions
            .Where(e => e.PayrollPeriodId == period.Id)
            .ExecuteDeleteAsync(cancellationToken);

        // 6. Reset period totals & metadata
        period.Status = PayrollPeriodStatus.Draft;
        period.TotalGrossPay = 0;
        period.TotalDeductions = 0;
        period.TotalEmployerContributions = 0;
        period.TotalNetPay = 0;
        period.TotalEmployeesProcessed = 0;
        period.TotalExceptionsCount = 0;
        period.CalculatedAt = null;
        period.ApprovedBy = null;
        period.ApprovedAt = null;
        period.LockedBy = null;
        period.LockedAt = null;

        await _context.SaveChangesAsync(cancellationToken);
        return period;
    }

    public async Task DeletePeriodAsync(int periodId, int userId, CancellationToken cancellationToken = default)
    {
        // First reset all child data (slices, items, payslips, un-stamp attendance)
        await ResetFinancialsAsync(periodId, userId, cancellationToken);

        var exists = await _context.PayrollPeriods
            .IgnoreQueryFilters()
            .AnyAsync(p => p.Id == periodId, cancellationToken);
        if (!exists)
            throw new InvalidOperationException($"Payroll period {periodId} not found.");

        await _context.PayrollPeriods
            .Where(p => p.Id == periodId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
