using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using HRAttendance.Business.Interfaces.Payroll;
using HRAttendance.Business.Interfaces.Payroll.Statutory;
using HRAttendance.Data;
using HRAttendance.Data.Models.Payroll;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Leave;

namespace HRAttendance.Business.Services.Payroll;

public class PayrollCalculationEngine : IPayrollCalculationEngine
{
    private readonly ApplicationDbContext _context;
    private readonly IEmploymentPeriodResolver _employmentPeriodResolver;
    private readonly IMidMonthProrationService _prorationService;
    private readonly ILOPCalculationService _lopService;
    private readonly IOvertimeCalculationService _overtimeService;
    private readonly IStatutoryCalculationService _statutoryService;

    public PayrollCalculationEngine(
        ApplicationDbContext context,
        IEmploymentPeriodResolver employmentPeriodResolver,
        IMidMonthProrationService prorationService,
        ILOPCalculationService lopService,
        IOvertimeCalculationService overtimeService,
        IStatutoryCalculationService statutoryService)
    {
        _context = context;
        _employmentPeriodResolver = employmentPeriodResolver;
        _prorationService = prorationService;
        _lopService = lopService;
        _overtimeService = overtimeService;
        _statutoryService = statutoryService;
    }

    public async Task<PayrollEmployee> CalculateEmployeePayrollAsync(int payrollPeriodId, int employeeId, CancellationToken cancellationToken = default)
    {
        var period = await _context.PayrollPeriods
            .Include(p => p.FinancialYear)
            .FirstOrDefaultAsync(p => p.Id == payrollPeriodId, cancellationToken)
            ?? throw new InvalidOperationException($"Payroll period {payrollPeriodId} not found.");

        if (period.Status == PayrollPeriodStatus.Locked)
        {
            throw new InvalidOperationException($"Payroll period {period.Month}/{period.Year} is Locked and cannot be modified.");
        }

        var policy = await _context.PayrollPolicies
            .FirstOrDefaultAsync(p => p.OrganizationId == period.OrganizationId, cancellationToken)
            ?? new PayrollPolicy { OrganizationId = period.OrganizationId };

        var employee = await _context.Employees
            .Include(e => e.ProfessionalDetails)
                .ThenInclude(pd => pd!.Location)
            .Include(e => e.EmployeeRoles)
            .Include(e => e.ContactDetails)
            .Include(e => e.SalaryStructures.Where(s => s.IsActive))
                .ThenInclude(s => s.Items)
                    .ThenInclude(i => i.Component)
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken)
            ?? throw new InvalidOperationException($"Employee {employeeId} not found.");

        // 1. Employment Period Resolution
        var window = _employmentPeriodResolver.Resolve(employee, period.Year, period.Month);

        // 2. Mid-Month Salary Revision Slices
        var structures = employee.SalaryStructures.ToList();
        var proration = _prorationService.CalculateSlices(window, structures, policy);

        // Check for missing salary structure exception
        if (structures.Count == 0)
        {
            _context.PayrollExceptions.Add(new PayrollException
            {
                PayrollPeriodId = period.Id,
                EmployeeId = employee.Id,
                Severity = ExceptionSeverity.Critical,
                ErrorCode = "NO_SALARY_STRUCTURE",
                Message = $"Employee {employee.FirstName} {employee.LastName} has no active salary structure assigned."
            });
        }

        // 3. Attendance & Leave Reconciliation
        var locationId = employee.ProfessionalDetails?.LocationId;
        var roleIds = employee.EmployeeRoles?.Select(r => r.RoleId).ToList() ?? new List<int>();

        var startDateTime = new DateTimeOffset(window.StartDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var endDateTime = new DateTimeOffset(window.EndDate.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        var attendances = await _context.EmployeeAttendances
            .Where(a => a.EmployeeId == employee.Id && a.InTime >= startDateTime && a.InTime <= endDateTime)
            .ToListAsync(cancellationToken);

        var subLeaves = await _context.SubLeaveApplications
            .Include(s => s.LeaveApplication)
                .ThenInclude(l => l.LeaveType)
                    .ThenInclude(lt => lt.LeaveSetting)
            .Where(s => s.EmployeeId == employee.Id
                     && s.LeaveDate >= window.StartDate
                     && s.LeaveDate <= window.EndDate
                     && s.Status == "Approved")
            .ToListAsync(cancellationToken);

        var holidays = await _context.Holidays
            .Where(h => h.OrganizationId == period.OrganizationId
                     && h.Date >= window.StartDate
                     && h.Date <= window.EndDate
                     && (h.LocationId == null || h.LocationId == locationId))
            .ToListAsync(cancellationToken);

        var offDayRules = await _context.OffDays
            .Where(o => o.OrganizationId == period.OrganizationId
                     && (locationId == null || o.LocationId == 0 || o.LocationId == locationId)
                     && (roleIds.Count == 0 || o.RoleId == 0 || roleIds.Contains(o.RoleId)))
            .ToListAsync(cancellationToken);

        decimal presentDays = 0m;
        decimal halfDays = 0m;
        decimal absentDays = 0m;
        decimal paidLeaveDays = 0m;
        decimal lopDays = 0m;
        decimal totalWorkingDaysInWindow = 0m;

        for (var d = window.StartDate; d <= window.EndDate; d = d.AddDays(1))
        {
            var isOff = CheckIfOffDay(d, offDayRules);
            var isHoliday = holidays.Any(h => h.Date == d);

            var dayPunch = attendances.FirstOrDefault(a => a.InTime.HasValue && DateOnly.FromDateTime(a.InTime.Value.DateTime) == d);
            var dayLeave = subLeaves.FirstOrDefault(s => s.LeaveDate == d);

            if (isOff || isHoliday)
            {
                // Non-working day (Weekly Off or Public Holiday)
                if (dayPunch != null)
                {
                    // Employee worked on off-day or holiday
                    if (dayPunch.Status == 3) presentDays += 0.5m;
                    else presentDays += 1m;
                }
                continue;
            }

            // Expected working day
            totalWorkingDaysInWindow += 1m;

            if (dayPunch != null)
            {
                if (dayPunch.Status == 3) // Half-Day punch
                {
                    halfDays += 1m;
                    presentDays += 0.5m;
                    if (dayLeave != null && (dayLeave.LeaveApplication?.LeaveType?.LeaveSetting?.IsPaid ?? true))
                    {
                        paidLeaveDays += 0.5m;
                    }
                    else
                    {
                        absentDays += 0.5m;
                        lopDays += 0.5m;
                    }
                }
                else
                {
                    // Full-Day Present / Late
                    presentDays += 1m;
                }
            }
            else if (dayLeave != null)
            {
                bool isPaid = dayLeave.LeaveApplication?.LeaveType?.LeaveSetting?.IsPaid ?? true;
                if (dayLeave.IsHalfDay)
                {
                    halfDays += 1m;
                    if (isPaid)
                    {
                        paidLeaveDays += 0.5m;
                        absentDays += 0.5m;
                        lopDays += 0.5m;
                    }
                    else
                    {
                        absentDays += 1m;
                        lopDays += 1m;
                    }
                }
                else
                {
                    if (isPaid)
                    {
                        paidLeaveDays += 1m;
                    }
                    else
                    {
                        absentDays += 1m;
                        lopDays += 1m;
                    }
                }
            }
            else
            {
                // No punch and no approved leave on a scheduled working day -> Unexcused Absence
                absentDays += 1m;
                lopDays += 1m;
            }
        }

        // Total working days (exact from calendar, at least 1)
        var totalWorkingDays = Math.Max(1m, totalWorkingDaysInWindow);

        // Entire-period absence guardrail:
        // If employee has 0 present days and 0 paid leaves, deduct 100% of salary for their eligible window
        if (presentDays == 0 && paidLeaveDays == 0 && window.EligibleDays > 0)
        {
            lopDays = policy.LOPBasis == LOPCalculationBasis.WorkingDays ? totalWorkingDays : (decimal)window.EligibleDays;
            absentDays = (decimal)window.EligibleDays;
        }

        // 4. LOP Deduction
        var lopResult = _lopService.Calculate(proration.BaseMonthlyGross, lopDays, window.TotalDaysInMonth, totalWorkingDays, policy);

        // 5. Overtime Calculation
        var overtimeQuery = _context.OTEntries
            .Where(o => o.EmployeeId == employee.Id
                     && o.OTDate >= window.StartDate
                     && o.OTDate <= window.EndDate);

        var otEntries = await overtimeQuery.ToListAsync(cancellationToken);
        decimal approvedOTHours = otEntries.Sum(o => o.OTHours);

        var basicComponent = proration.ComponentProrations.FirstOrDefault(c => c.ComponentCode.Equals("BASIC", StringComparison.OrdinalIgnoreCase));
        decimal basicAmount = basicComponent?.ProratedAmount ?? (proration.ProratedGross * 0.5m);

        var otResult = _overtimeService.Calculate(basicAmount, proration.ProratedGross, approvedOTHours, policy);

        // 6. Adjustments & Arrears
        var adjustments = await _context.PayrollAdjustments
            .Where(a => a.PayrollPeriodId == period.Id && a.EmployeeId == employee.Id && a.Status == AdjustmentStatus.Approved)
            .ToListAsync(cancellationToken);

        decimal earningAdjustments = adjustments.Where(a => a.Direction == AdjustmentDirection.Earning).Sum(a => a.Amount);
        decimal deductionAdjustments = adjustments.Where(a => a.Direction == AdjustmentDirection.Deduction).Sum(a => a.Amount);

        var arrears = await _context.PayrollArrears
            .Where(a => a.TargetPayrollPeriodId == period.Id && a.EmployeeId == employee.Id && a.Status == ArrearStatus.Pending)
            .ToListAsync(cancellationToken);

        decimal arrearsTotal = arrears.Sum(a => a.DifferenceAmount);

        // 7. Statutory Deductions (Calculated on Earned Gross/Basic after LOP)
        var stateCode = employee.ContactDetails?.State ?? "DEFAULT";
        decimal earnedGross = Math.Max(0m, proration.ProratedGross - lopResult.LOPDeduction);
        decimal earnedBasic = proration.ProratedGross > 0 ? Math.Round(basicAmount * (earnedGross / proration.ProratedGross), 2) : 0m;

        var statutoryResult = await _statutoryService.CalculateStatutoryAsync(
            organizationId: period.OrganizationId,
            employeeId: employee.Id,
            stateCode: stateCode,
            basicSalary: earnedBasic,
            grossSalary: earnedGross,
            year: period.Year,
            month: period.Month,
            calculationDate: new DateOnly(period.Year, period.Month, window.TotalDaysInMonth)
        );

        // 8. Financial Totals
        decimal grossEarnings = proration.ProratedGross + otResult.OvertimePay + earningAdjustments + arrearsTotal;
        decimal totalDeductions = lopResult.LOPDeduction + statutoryResult.TotalEmployeeDeductions + deductionAdjustments;
        decimal netPay = grossEarnings - totalDeductions;

        // 9. Pre-Flight Exception Checks
        if (netPay < 0)
        {
            _context.PayrollExceptions.Add(new PayrollException
            {
                PayrollPeriodId = period.Id,
                EmployeeId = employee.Id,
                Severity = ExceptionSeverity.Critical,
                ErrorCode = "NEGATIVE_NET_PAY",
                Message = $"Employee {employee.FirstName} {employee.LastName} has negative Net Pay (Gross: ₹{grossEarnings:N2}, Deductions: ₹{totalDeductions:N2}, Net: ₹{netPay:N2})."
            });
        }

        // 10. Assemble Itemized Payroll Items
        var items = new List<PayrollItem>();

        // A. Prorated Components (Earnings)
        foreach (var comp in proration.ComponentProrations.Where(c => c.Type == ComponentType.Earning))
        {
            items.Add(new PayrollItem
            {
                ComponentCode = comp.ComponentCode,
                ComponentName = comp.ComponentName,
                ComponentType = ComponentType.Earning,
                CalculationOrder = comp.CalculationOrder,
                CalculationType = comp.CalculationType,
                CalculationBase = comp.FullMonthlyAmount,
                CalculationRate = 0m,
                OriginalAmount = comp.FullMonthlyAmount,
                ProratedAmount = comp.ProratedAmount,
                FinalAmount = comp.ProratedAmount,
                CalculationFormula = comp.DerivationFormula,
                CalculationNotes = comp.DerivationNotes
            });
        }

        // B. Overtime Pay
        if (otResult.OvertimePay > 0)
        {
            items.Add(new PayrollItem
            {
                ComponentCode = "OVERTIME",
                ComponentName = "Overtime Pay",
                ComponentType = ComponentType.Earning,
                CalculationOrder = 4,
                CalculationType = ComponentCalculationType.Formula,
                CalculationBase = otResult.HourlyRate,
                CalculationRate = approvedOTHours,
                OriginalAmount = otResult.OvertimePay,
                ProratedAmount = otResult.OvertimePay,
                FinalAmount = otResult.OvertimePay,
                CalculationFormula = otResult.Formula,
                CalculationNotes = otResult.Notes
            });
        }

        // C. LOP Deduction
        if (lopResult.LOPDeduction > 0)
        {
            items.Add(new PayrollItem
            {
                ComponentCode = "LOP_DEDUCTION",
                ComponentName = "Loss of Pay (Absence)",
                ComponentType = ComponentType.Deduction,
                CalculationOrder = 4,
                CalculationType = ComponentCalculationType.Formula,
                CalculationBase = lopResult.DailyRate,
                CalculationRate = lopDays,
                OriginalAmount = lopResult.LOPDeduction,
                ProratedAmount = lopResult.LOPDeduction,
                FinalAmount = lopResult.LOPDeduction,
                CalculationFormula = lopResult.Formula,
                CalculationNotes = lopResult.Notes
            });
        }

        // D. Statutory Items
        items.AddRange(statutoryResult.GeneratedItems);

        // E. Adjustments Items
        foreach (var adj in adjustments)
        {
            items.Add(new PayrollItem
            {
                ComponentCode = $"ADJ_{adj.Type.ToString().ToUpperInvariant()}",
                ComponentName = $"{adj.Type} ({adj.AdjustmentNumber})",
                ComponentType = adj.Direction == AdjustmentDirection.Earning ? ComponentType.Earning : ComponentType.Deduction,
                CalculationOrder = 9,
                CalculationType = ComponentCalculationType.ManualAmount,
                CalculationBase = adj.Amount,
                CalculationRate = 0m,
                OriginalAmount = adj.Amount,
                ProratedAmount = adj.Amount,
                FinalAmount = adj.Amount,
                CalculationFormula = $"Adjustment {adj.AdjustmentNumber}",
                CalculationNotes = adj.Reason
            });
        }

        // 11. Create or Update PayrollEmployee
        var existingPayrollEmp = await _context.PayrollEmployees
            .FirstOrDefaultAsync(pe => pe.PayrollPeriodId == period.Id && pe.EmployeeId == employee.Id, cancellationToken);

        var calcVersion = existingPayrollEmp != null ? existingPayrollEmp.CalculationVersion + 1 : 1;

        if (existingPayrollEmp != null)
        {
            await _context.PayrollSalarySlices
                .Where(s => s.PayrollEmployeeId == existingPayrollEmp.Id)
                .ExecuteDeleteAsync(cancellationToken);
            await _context.PayrollItems
                .Where(i => i.PayrollEmployeeId == existingPayrollEmp.Id)
                .ExecuteDeleteAsync(cancellationToken);
        }

        var payrollEmp = existingPayrollEmp ?? new PayrollEmployee
        {
            PayrollPeriodId = period.Id,
            EmployeeId = employee.Id,
            OrganizationId = period.OrganizationId
        };

        payrollEmp.CalculationVersion = calcVersion;
        payrollEmp.EligibleEmploymentDays = window.EligibleDays;
        payrollEmp.CalendarDaysInMonth = window.TotalDaysInMonth;
        payrollEmp.WorkingDays = totalWorkingDays;
        payrollEmp.PresentDays = presentDays;
        payrollEmp.PaidLeaveDays = paidLeaveDays;
        payrollEmp.HalfDays = halfDays;
        payrollEmp.LOPDays = lopDays;
        payrollEmp.ApprovedOvertimeHours = approvedOTHours;

        payrollEmp.BaseMonthlyGross = proration.BaseMonthlyGross;
        payrollEmp.ProratedGross = proration.ProratedGross;
        payrollEmp.LOPDeduction = lopResult.LOPDeduction;
        payrollEmp.OvertimePay = otResult.OvertimePay;
        payrollEmp.AdjustmentsTotal = earningAdjustments - deductionAdjustments;
        payrollEmp.ArrearsTotal = arrearsTotal;
        payrollEmp.GrossEarnings = grossEarnings;
        payrollEmp.StatutoryDeductions = statutoryResult.TotalEmployeeDeductions;
        payrollEmp.OtherDeductions = deductionAdjustments;
        payrollEmp.TotalDeductions = totalDeductions;
        payrollEmp.EmployerContributions = statutoryResult.TotalEmployerContributions;
        payrollEmp.NetPay = netPay;
        payrollEmp.Status = PayrollEmployeeStatus.Calculated;
        payrollEmp.CalculatedAt = DateTimeOffset.UtcNow;

        if (existingPayrollEmp == null)
        {
            _context.PayrollEmployees.Add(payrollEmp);
        }

        // Link child slices and items
        payrollEmp.Slices.Clear();
        payrollEmp.Items.Clear();

        foreach (var slice in proration.Slices)
        {
            slice.PayrollEmployee = payrollEmp;
            payrollEmp.Slices.Add(slice);
        }

        foreach (var item in items)
        {
            item.PayrollEmployee = payrollEmp;
            payrollEmp.Items.Add(item);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return payrollEmp;
    }

    public async Task<PayrollCalculationBatchResult> CalculatePeriodBatchAsync(int payrollPeriodId, CancellationToken cancellationToken = default)
    {
        var period = await _context.PayrollPeriods
            .Include(p => p.FinancialYear)
            .FirstOrDefaultAsync(p => p.Id == payrollPeriodId, cancellationToken)
            ?? throw new InvalidOperationException($"Payroll period {payrollPeriodId} not found.");

        if (period.Status == PayrollPeriodStatus.Locked)
        {
            throw new InvalidOperationException($"Payroll period {period.Month}/{period.Year} is Locked and cannot be modified.");
        }

        // Heal dates if they were default / invalid
        if (period.StartDate.Year <= 1)
            period.StartDate = new DateOnly(period.Year, period.Month, 1);
        if (period.EndDate.Year <= 1)
            period.EndDate = new DateOnly(period.Year, period.Month, DateTime.DaysInMonth(period.Year, period.Month));

        period.Status = PayrollPeriodStatus.Calculating;
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            var policy = await _context.PayrollPolicies
                .FirstOrDefaultAsync(p => p.OrganizationId == period.OrganizationId, cancellationToken)
                ?? new PayrollPolicy { OrganizationId = period.OrganizationId };

            // Freeze Policy Snapshot
            period.PolicySnapshotJson = JsonSerializer.Serialize(new
            {
                ProrationBasis = policy.ProrationBasis.ToString(),
                FixedProrationDays = policy.FixedProrationDays,
                LOPBasis = policy.LOPBasis.ToString(),
                FixedLOPDays = policy.FixedLOPDays,
                OTBasis = policy.OTBasis.ToString(),
                OTMultiplier = policy.OTMultiplier,
                StandardMonthlyWorkingHours = policy.StandardMonthlyWorkingHours,
                RoundingRule = policy.RoundingRule.ToString(),
                CalculatedAtUtc = DateTimeOffset.UtcNow
            });

            // Clear existing unresolved exceptions for recalculations
            var oldExceptions = await _context.PayrollExceptions
                .Where(e => e.PayrollPeriodId == period.Id && !e.IsResolved)
                .ToListAsync(cancellationToken);
            _context.PayrollExceptions.RemoveRange(oldExceptions);

            // Fetch active employees in organization
            var employees = await _context.Employees
                .Where(e => e.OrganizationId == period.OrganizationId && e.IsActive)
                .Select(e => e.Id)
                .ToListAsync(cancellationToken);

            int succeeded = 0;
            decimal totalGross = 0m;
            decimal totalDeductions = 0m;
            decimal totalNet = 0m;
            decimal totalEmployerContributions = 0m;

            foreach (var empId in employees)
            {
                try
                {
                    var payrollEmp = await CalculateEmployeePayrollAsync(period.Id, empId, cancellationToken);
                    totalGross += payrollEmp.GrossEarnings;
                    totalDeductions += payrollEmp.TotalDeductions;
                    totalNet += payrollEmp.NetPay;
                    totalEmployerContributions += payrollEmp.EmployerContributions;
                    succeeded++;
                }
                catch (Exception ex)
                {
                    _context.PayrollExceptions.Add(new PayrollException
                    {
                        PayrollPeriodId = period.Id,
                        EmployeeId = empId,
                        Severity = ExceptionSeverity.Critical,
                        ErrorCode = "CALCULATION_ENGINE_ERROR",
                        Message = $"Error calculating payroll for employee {empId}: {ex.Message}"
                    });
                }
            }

            var exceptionCount = await _context.PayrollExceptions
                .CountAsync(e => e.PayrollPeriodId == period.Id && !e.IsResolved, cancellationToken);

            period.TotalGrossPay = totalGross;
            period.TotalDeductions = totalDeductions;
            period.TotalNetPay = totalNet;
            period.TotalEmployerContributions = totalEmployerContributions;
            period.TotalEmployeesProcessed = succeeded;
            period.TotalExceptionsCount = exceptionCount;
            period.CalculatedAt = DateTimeOffset.UtcNow;
            period.Status = PayrollPeriodStatus.Calculated;

            await _context.SaveChangesAsync(cancellationToken);

            return new PayrollCalculationBatchResult(
                PayrollPeriodId: period.Id,
                TotalProcessed: employees.Count,
                TotalSucceeded: succeeded,
                TotalExceptions: exceptionCount,
                TotalGrossPay: totalGross,
                TotalDeductions: totalDeductions,
                TotalNetPay: totalNet,
                PolicySnapshotJson: period.PolicySnapshotJson
            );
        }
        catch (Exception)
        {
            _context.ChangeTracker.Clear();
            var p = await _context.PayrollPeriods.FindAsync(new object[] { payrollPeriodId }, CancellationToken.None);
            if (p != null)
            {
                p.Status = PayrollPeriodStatus.Draft;
                await _context.SaveChangesAsync(CancellationToken.None);
            }
            throw;
        }
    }

    private static bool CheckIfOffDay(DateOnly date, List<OffDay> offDayRules)
    {
        var dayName = date.DayOfWeek.ToString();
        var weekNumber = ((date.Day - 1) / 7) + 1; // 1, 2, 3, 4, 5

        if (offDayRules != null && offDayRules.Count > 0)
        {
            foreach (var rule in offDayRules)
            {
                if (rule.OffDayName.Equals(dayName, StringComparison.OrdinalIgnoreCase))
                {
                    bool isOff = weekNumber switch
                    {
                        1 => rule.Week1,
                        2 => rule.Week2,
                        3 => rule.Week3,
                        4 => rule.Week4,
                        5 => rule.Week5,
                        _ => rule.Week6
                    };

                    if (isOff) return true;
                }
            }
        }
        else
        {
            // Default fallback if no custom off-day rules are configured in the organization: Sunday is weekly off
            return date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday;
        }

        return false;
    }
}
