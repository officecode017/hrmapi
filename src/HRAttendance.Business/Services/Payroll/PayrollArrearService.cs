using Microsoft.EntityFrameworkCore;
using HRAttendance.Business.Interfaces.Payroll;
using HRAttendance.Data;
using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Services.Payroll;

public class PayrollArrearService : IPayrollArrearService
{
    private readonly ApplicationDbContext _context;

    public PayrollArrearService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PayrollArrear> CreateArrearAsync(CreateArrearDto dto, int userId, CancellationToken cancellationToken = default)
    {
        var targetPeriod = await _context.PayrollPeriods
            .FirstOrDefaultAsync(p => p.Id == dto.TargetPayrollPeriodId, cancellationToken)
            ?? throw new InvalidOperationException($"Target payroll period {dto.TargetPayrollPeriodId} not found.");

        if (targetPeriod.Status == PayrollPeriodStatus.Locked ||
            targetPeriod.Status == PayrollPeriodStatus.PaymentProcessing ||
            targetPeriod.Status == PayrollPeriodStatus.Paid)
        {
            throw new InvalidOperationException($"Cannot create arrears for a target period in '{targetPeriod.Status}' status.");
        }

        var year = DateTime.UtcNow.Year;
        var count = await _context.PayrollArrears
            .CountAsync(a => a.OrganizationId == dto.OrganizationId, cancellationToken);
        var arrearNumber = $"ARR-{year}-{(count + 1):D6}";

        var difference = dto.CorrectAmount - dto.OriginalAmount;

        var arrear = new PayrollArrear
        {
            ArrearNumber = arrearNumber,
            OrganizationId = dto.OrganizationId,
            EmployeeId = dto.EmployeeId,
            SourcePayrollPeriodId = dto.SourcePayrollPeriodId,
            TargetPayrollPeriodId = dto.TargetPayrollPeriodId,
            ComponentCode = dto.ComponentCode,
            OriginalAmount = dto.OriginalAmount,
            CorrectAmount = dto.CorrectAmount,
            DifferenceAmount = difference,
            Reason = dto.Reason,
            Status = ArrearStatus.Pending,
            CreatedBy = userId
        };

        _context.PayrollArrears.Add(arrear);
        await _context.SaveChangesAsync(cancellationToken);

        return arrear;
    }

    public async Task<PayrollArrear> CalculateArrearDifferenceAsync(int arrearId, CancellationToken cancellationToken = default)
    {
        var arrear = await _context.PayrollArrears
            .FirstOrDefaultAsync(a => a.Id == arrearId, cancellationToken)
            ?? throw new InvalidOperationException($"Arrear {arrearId} not found.");

        arrear.DifferenceAmount = arrear.CorrectAmount - arrear.OriginalAmount;
        if (arrear.Status == ArrearStatus.Pending)
        {
            arrear.Status = ArrearStatus.Calculated;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return arrear;
    }

    public async Task<PayrollArrear> ApplyArrearAsync(int arrearId, int targetPeriodId, CancellationToken cancellationToken = default)
    {
        var arrear = await _context.PayrollArrears
            .FirstOrDefaultAsync(a => a.Id == arrearId, cancellationToken)
            ?? throw new InvalidOperationException($"Arrear {arrearId} not found.");

        arrear.TargetPayrollPeriodId = targetPeriodId;
        arrear.Status = ArrearStatus.Applied;

        await _context.SaveChangesAsync(cancellationToken);
        return arrear;
    }

    public async Task<PayrollArrear> CancelArrearAsync(int arrearId, int userId, CancellationToken cancellationToken = default)
    {
        var arrear = await _context.PayrollArrears
            .FirstOrDefaultAsync(a => a.Id == arrearId, cancellationToken)
            ?? throw new InvalidOperationException($"Arrear {arrearId} not found.");

        if (arrear.Status == ArrearStatus.Applied)
        {
            throw new InvalidOperationException("Cannot cancel an already applied arrear.");
        }

        arrear.Status = ArrearStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);
        return arrear;
    }

    public async Task<IReadOnlyList<PayrollArrear>> GetArrearsByTargetPeriodAsync(int targetPeriodId, CancellationToken cancellationToken = default)
    {
        return await _context.PayrollArrears
            .Include(a => a.Employee)
            .Where(a => a.TargetPayrollPeriodId == targetPeriodId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
