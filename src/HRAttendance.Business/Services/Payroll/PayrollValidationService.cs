using Microsoft.EntityFrameworkCore;
using HRAttendance.Business.Interfaces.Payroll;
using HRAttendance.Data;
using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Services.Payroll;

public class PayrollValidationService : IPayrollValidationService
{
    private readonly ApplicationDbContext _context;

    public PayrollValidationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ValidationSummary> ValidatePeriodAsync(int payrollPeriodId, CancellationToken cancellationToken = default)
    {
        var period = await _context.PayrollPeriods
            .Include(p => p.Employees)
                .ThenInclude(pe => pe.Employee)
                    .ThenInclude(e => e.ContactDetails)
            .Include(p => p.Employees)
                .ThenInclude(pe => pe.Slices)
            .FirstOrDefaultAsync(p => p.Id == payrollPeriodId, cancellationToken)
            ?? throw new InvalidOperationException($"Payroll period {payrollPeriodId} not found.");

        // Clear existing unresolved exceptions to prevent duplication on re-validation
        var existingExceptions = await _context.PayrollExceptions
            .Where(e => e.PayrollPeriodId == period.Id && !e.IsResolved)
            .ToListAsync(cancellationToken);

        _context.PayrollExceptions.RemoveRange(existingExceptions);

        var newExceptions = new List<PayrollException>();

        foreach (var pe in period.Employees)
        {
            // 1. Critical: Negative Net Pay
            if (pe.NetPay < 0)
            {
                newExceptions.Add(new PayrollException
                {
                    PayrollPeriodId = period.Id,
                    EmployeeId = pe.EmployeeId,
                    Severity = ExceptionSeverity.Critical,
                    ErrorCode = "NEGATIVE_NET_PAY",
                    Message = $"Employee {pe.Employee.FirstName} {pe.Employee.LastName} has negative Net Pay (₹{pe.NetPay:N2}). Gross earnings: ₹{pe.GrossEarnings:N2}, Total deductions: ₹{pe.TotalDeductions:N2}."
                });
            }

            // 2. Critical: Missing Salary Slices / Structure
            if (pe.Slices.Count == 0 && pe.EligibleEmploymentDays > 0)
            {
                newExceptions.Add(new PayrollException
                {
                    PayrollPeriodId = period.Id,
                    EmployeeId = pe.EmployeeId,
                    Severity = ExceptionSeverity.Critical,
                    ErrorCode = "NO_SALARY_STRUCTURE",
                    Message = $"Employee {pe.Employee.FirstName} {pe.Employee.LastName} has no active salary structure configured for this period."
                });
            }

            // 3. Warning: Missing Bank Account Details
            // Note: Bank details will be looked up from Employee contact/banking info
            if (string.IsNullOrWhiteSpace(pe.Employee.ContactDetails?.Mobile) && string.IsNullOrWhiteSpace(pe.Employee.ContactDetails?.WorkEmail))
            {
                newExceptions.Add(new PayrollException
                {
                    PayrollPeriodId = period.Id,
                    EmployeeId = pe.EmployeeId,
                    Severity = ExceptionSeverity.Warning,
                    ErrorCode = "INCOMPLETE_CONTACT_DETAILS",
                    Message = $"Employee {pe.Employee.FirstName} {pe.Employee.LastName} has missing contact details required for disbursement communication."
                });
            }
        }

        if (newExceptions.Count > 0)
        {
            _context.PayrollExceptions.AddRange(newExceptions);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var allExceptions = await _context.PayrollExceptions
            .Where(e => e.PayrollPeriodId == period.Id && !e.IsResolved)
            .ToListAsync(cancellationToken);

        var criticalCount = allExceptions.Count(e => e.Severity == ExceptionSeverity.Critical);
        var warningCount = allExceptions.Count(e => e.Severity == ExceptionSeverity.Warning);
        var infoCount = allExceptions.Count(e => e.Severity == ExceptionSeverity.Information);

        return new ValidationSummary(
            TotalExceptions: allExceptions.Count,
            CriticalCount: criticalCount,
            WarningCount: warningCount,
            InfoCount: infoCount,
            CanApproveAndLock: criticalCount == 0,
            Exceptions: allExceptions
        );
    }

    public async Task<PayrollException> ResolveExceptionAsync(int exceptionId, int resolvedByUserId, string notes, CancellationToken cancellationToken = default)
    {
        var ex = await _context.PayrollExceptions
            .FirstOrDefaultAsync(e => e.Id == exceptionId, cancellationToken)
            ?? throw new InvalidOperationException($"Payroll exception {exceptionId} not found.");

        ex.IsResolved = true;
        ex.ResolvedBy = resolvedByUserId;
        ex.ResolvedAt = DateTimeOffset.UtcNow;
        ex.ResolutionNotes = notes;

        await _context.SaveChangesAsync(cancellationToken);
        return ex;
    }
}
