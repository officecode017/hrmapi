using Microsoft.EntityFrameworkCore;
using HRAttendance.Business.Interfaces.Payroll;
using HRAttendance.Data;
using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Services.Payroll;

public class PayslipProjectionService : IPayslipProjectionService
{
    private readonly ApplicationDbContext _context;

    public PayslipProjectionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Payslip> ProjectSinglePayslipAsync(int payrollEmployeeId, CancellationToken cancellationToken = default)
    {
        var pe = await _context.PayrollEmployees
            .Include(p => p.Period)
            .Include(p => p.Items)
            .Include(p => p.Payslip)
                .ThenInclude(ps => ps!.Items)
            .FirstOrDefaultAsync(p => p.Id == payrollEmployeeId, cancellationToken)
            ?? throw new InvalidOperationException($"PayrollEmployee {payrollEmployeeId} not found.");

        var payslip = ProjectPayslipInternal(pe);
        await _context.SaveChangesAsync(cancellationToken);
        return payslip;
    }

    public async Task<IReadOnlyList<Payslip>> ProjectPayslipsForPeriodAsync(int payrollPeriodId, CancellationToken cancellationToken = default)
    {
        var payrollEmployees = await _context.PayrollEmployees
            .Include(p => p.Period)
            .Include(p => p.Items)
            .Include(p => p.Payslip)
                .ThenInclude(ps => ps!.Items)
            .Where(p => p.PayrollPeriodId == payrollPeriodId)
            .ToListAsync(cancellationToken);

        var payslips = new List<Payslip>();

        foreach (var pe in payrollEmployees)
        {
            var payslip = ProjectPayslipInternal(pe);
            payslips.Add(payslip);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return payslips;
    }

    private Payslip ProjectPayslipInternal(PayrollEmployee pe)
    {
        var payslipNumber = $"PAY-{pe.Period.Year}{pe.Period.Month:D2}-{pe.EmployeeId:D6}";

        Payslip payslip;
        if (pe.Payslip == null)
        {
            payslip = new Payslip
            {
                PayrollEmployeeId = pe.Id,
                EmployeeId = pe.EmployeeId,
                PayslipNumber = payslipNumber,
                Status = PayslipStatus.Generated,
                GeneratedAt = DateTimeOffset.UtcNow
            };
            _context.Payslips.Add(payslip);
            pe.Payslip = payslip;
        }
        else
        {
            payslip = pe.Payslip;
            payslip.PayslipNumber = payslipNumber;
            payslip.Status = PayslipStatus.Generated;
            payslip.GeneratedAt = DateTimeOffset.UtcNow;

            // Remove existing items to project a fresh 1:1 view
            _context.PayslipItems.RemoveRange(payslip.Items);
            payslip.Items.Clear();
        }

        // Project items 1:1 from PayrollItem records
        foreach (var item in pe.Items.OrderBy(i => i.CalculationOrder))
        {
            var payslipItem = new PayslipItem
            {
                Payslip = payslip,
                ComponentCode = item.ComponentCode,
                ComponentName = item.ComponentName,
                ComponentType = item.ComponentType,
                Amount = item.FinalAmount,
                DisplayOrder = item.CalculationOrder
            };

            payslip.Items.Add(payslipItem);
            _context.PayslipItems.Add(payslipItem);
        }

        return payslip;
    }
}
