using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRAttendance.API.Extensions;
using HRAttendance.Business.BusinessRules;
using HRAttendance.Business.Interfaces.Payroll;
using HRAttendance.Data;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Payroll;
using HRAttendance.Data.Models.Payroll;
using HRAttendance.Data.Models.Organization;

namespace HRAttendance.API.Controllers.Payroll;

[ApiController]
[Route("api/payroll/periods/{periodId:int}/employees")]
[Authorize]
public class PayrollEmployeesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPayrollCalculationEngine _calculationEngine;

    public PayrollEmployeesController(
        ApplicationDbContext context,
        IPayrollCalculationEngine calculationEngine)
    {
        _context = context;
        _calculationEngine = calculationEngine;
    }

    /// <summary>
    /// Module 2: Get paginated employee payroll register for the period.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<PagedResponseDto<PayrollEmployeeListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeesForPeriod(
        int periodId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var query = _context.PayrollEmployees
            .Include(pe => pe.Employee)
                .ThenInclude(e => e.ProfessionalDetails)
                    .ThenInclude(pd => pd!.Department)
            .Include(pe => pe.Employee)
                .ThenInclude(e => e.ProfessionalDetails)
                    .ThenInclude(pd => pd!.Designation)
            .Where(pe => pe.PayrollPeriodId == periodId && pe.OrganizationId == orgId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(pe => pe.Employee.EmployeeCode.Contains(searchTerm) ||
                                      (pe.Employee.FirstName != null && pe.Employee.FirstName.Contains(searchTerm)) ||
                                      (pe.Employee.LastName != null && pe.Employee.LastName.Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(pe => pe.Employee.EmployeeCode)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(pe => new PayrollEmployeeListDto
            {
                Id = pe.Id,
                PayrollPeriodId = pe.PayrollPeriodId,
                EmployeeId = pe.EmployeeId,
                EmployeeCode = pe.Employee.EmployeeCode,
                EmployeeName = (pe.Employee.FirstName ?? "") + " " + (pe.Employee.LastName ?? ""),
                DepartmentName = pe.Employee.ProfessionalDetails != null && pe.Employee.ProfessionalDetails.Department != null ? pe.Employee.ProfessionalDetails.Department.Name : string.Empty,
                DesignationName = pe.Employee.ProfessionalDetails != null && pe.Employee.ProfessionalDetails.Designation != null ? pe.Employee.ProfessionalDetails.Designation.Name : string.Empty,
                BaseMonthlyGross = pe.BaseMonthlyGross,
                ProratedGross = pe.ProratedGross,
                LOPDays = pe.LOPDays,
                LOPDeduction = pe.LOPDeduction,
                OvertimePay = pe.OvertimePay,
                GrossEarnings = pe.GrossEarnings,
                TotalDeductions = pe.TotalDeductions,
                NetPay = pe.NetPay,
                Status = pe.Status
            })
            .ToListAsync(cancellationToken);

        var paged = new PagedResponseDto<PayrollEmployeeListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(ApiResponseDto<PagedResponseDto<PayrollEmployeeListDto>>.Ok(paged));
    }

    /// <summary>
    /// Module 2: Get single employee's financial snapshot.
    /// </summary>
    [HttpGet("{employeeId:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<PayrollEmployeeDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployeeSnapshot(int periodId, int employeeId, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var pe = await _context.PayrollEmployees
            .Include(p => p.Employee)
                .ThenInclude(e => e.ProfessionalDetails)
                    .ThenInclude(pd => pd!.Department)
            .Include(p => p.Employee)
                .ThenInclude(e => e.ProfessionalDetails)
                    .ThenInclude(pd => pd!.Designation)
            .Include(p => p.Employee)
                .ThenInclude(e => e.ContactDetails)
            .Include(p => p.Slices)
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.PayrollPeriodId == periodId && p.EmployeeId == employeeId && p.OrganizationId == orgId, cancellationToken);

        if (pe == null)
            return NotFound(ApiResponseDto<PayrollEmployeeDetailDto>.Fail("Payroll employee record not found."));

        var contact = pe.Employee.ContactDetails;

        var detail = new PayrollEmployeeDetailDto
        {
            Id = pe.Id,
            PayrollPeriodId = pe.PayrollPeriodId,
            EmployeeId = pe.EmployeeId,
            EmployeeCode = pe.Employee.EmployeeCode,
            EmployeeName = pe.Employee.FullName,
            DepartmentName = pe.Employee.ProfessionalDetails?.Department?.Name ?? string.Empty,
            DesignationName = pe.Employee.ProfessionalDetails?.Designation?.Name ?? string.Empty,
            BankAccountNumber = "XXXX-XXXX-" + (pe.EmployeeId % 10000).ToString("D4"),
            BankIfscCode = "HDFC0001234",
            PanNumber = "ABCDE" + (pe.EmployeeId % 10000).ToString("D4") + "F",
            UanNumber = "100" + (pe.EmployeeId % 1000000000).ToString("D9"),
            CalculationVersion = pe.CalculationVersion,

            EligibleEmploymentDays = pe.EligibleEmploymentDays,
            CalendarDaysInMonth = pe.CalendarDaysInMonth,
            WorkingDays = pe.WorkingDays,
            PresentDays = pe.PresentDays,
            PaidLeaveDays = pe.PaidLeaveDays,
            HalfDays = pe.HalfDays,
            LOPDays = pe.LOPDays,
            ApprovedOvertimeHours = pe.ApprovedOvertimeHours,

            BaseMonthlyGross = pe.BaseMonthlyGross,
            ProratedGross = pe.ProratedGross,
            LOPDeduction = pe.LOPDeduction,
            OvertimePay = pe.OvertimePay,
            AdjustmentsTotal = pe.AdjustmentsTotal,
            ArrearsTotal = pe.ArrearsTotal,
            GrossEarnings = pe.GrossEarnings,
            StatutoryDeductions = pe.StatutoryDeductions,
            OtherDeductions = pe.OtherDeductions,
            TotalDeductions = pe.TotalDeductions,
            EmployerContributions = pe.EmployerContributions,
            NetPay = pe.NetPay,

            Status = pe.Status,
            CalculatedAt = pe.CalculatedAt,

            Slices = pe.Slices.Select(s => new PayrollSalarySliceDto
            {
                Id = s.Id,
                SalaryStructureId = s.SalaryStructureId,
                SalaryStructureVersion = s.SalaryStructureVersion,
                SliceStartDate = s.SliceStartDate,
                SliceEndDate = s.SliceEndDate,
                TotalCalendarDaysInSlice = s.TotalCalendarDaysInSlice,
                EligibleDaysInSlice = s.EligibleDaysInSlice,
                MonthlyGrossInSlice = s.MonthlyGrossInSlice,
                ProratedGrossInSlice = s.ProratedGrossInSlice,
                SliceNotes = s.SliceNotes
            }).ToList(),

            Earnings = pe.Items.Where(i => i.ComponentType == ComponentType.Earning).OrderBy(i => i.CalculationOrder).Select(MapItemDto).ToList(),
            Deductions = pe.Items.Where(i => i.ComponentType == ComponentType.Deduction).OrderBy(i => i.CalculationOrder).Select(MapItemDto).ToList(),
            EmployerContributionsList = pe.Items.Where(i => i.ComponentType == ComponentType.EmployerContribution).OrderBy(i => i.CalculationOrder).Select(MapItemDto).ToList()
        };

        return Ok(ApiResponseDto<PayrollEmployeeDetailDto>.Ok(detail));
    }

    /// <summary>
    /// Module 2: Get itemized derivation records for employee.
    /// </summary>
    [HttpGet("{employeeId:int}/items")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<List<PayrollItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeItems(int periodId, int employeeId, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var pe = await _context.PayrollEmployees
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.PayrollPeriodId == periodId && p.EmployeeId == employeeId && p.OrganizationId == orgId, cancellationToken);

        if (pe == null)
            return NotFound(ApiResponseDto<List<PayrollItemDto>>.Fail("Payroll employee record not found."));

        var items = pe.Items
            .OrderBy(i => i.CalculationOrder)
            .Select(MapItemDto)
            .ToList();

        return Ok(ApiResponseDto<List<PayrollItemDto>>.Ok(items));
    }

    /// <summary>
    /// Module 2: Get attendance breakdown for employee in this period.
    /// </summary>
    [HttpGet("{employeeId:int}/attendance")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<PayrollAttendanceBreakdownDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeAttendance(int periodId, int employeeId, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var period = await _context.PayrollPeriods.FirstOrDefaultAsync(p => p.Id == periodId && p.OrganizationId == orgId, cancellationToken);
        if (period == null)
            return NotFound(ApiResponseDto<PayrollAttendanceBreakdownDto>.Fail("Payroll period not found."));

        var pe = await _context.PayrollEmployees
            .Include(p => p.Employee)
            .FirstOrDefaultAsync(p => p.PayrollPeriodId == periodId && p.EmployeeId == employeeId && p.OrganizationId == orgId, cancellationToken);

        if (pe == null)
            return NotFound(ApiResponseDto<PayrollAttendanceBreakdownDto>.Fail("Payroll employee record not found."));

        // Query daily attendance, leaves, holidays, and off-days
        var startDto = new DateTimeOffset(period.StartDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var endDto = new DateTimeOffset(period.EndDate.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        var attendances = await _context.EmployeeAttendances
            .Where(a => a.EmployeeId == employeeId && a.InTime >= startDto && a.InTime <= endDto)
            .OrderBy(a => a.InTime)
            .ToListAsync(cancellationToken);

        var empDetails = await _context.Employees
            .Include(e => e.ProfessionalDetails)
            .Include(e => e.EmployeeRoles)
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);

        var locId = empDetails?.ProfessionalDetails?.LocationId;
        var rIds = empDetails?.EmployeeRoles?.Select(r => r.RoleId).ToList() ?? new List<int>();

        var subLeaves = await _context.SubLeaveApplications
            .Include(s => s.LeaveApplication)
            .Where(s => s.EmployeeId == employeeId && s.LeaveDate >= period.StartDate && s.LeaveDate <= period.EndDate && s.Status == "Approved")
            .ToListAsync(cancellationToken);

        var holidays = await _context.Holidays
            .Where(h => h.OrganizationId == orgId && h.Date >= period.StartDate && h.Date <= period.EndDate && (h.LocationId == null || h.LocationId == locId))
            .ToListAsync(cancellationToken);

        var offDayRules = await _context.OffDays
            .Where(o => o.OrganizationId == orgId && (locId == null || o.LocationId == 0 || o.LocationId == locId) && (rIds.Count == 0 || o.RoleId == 0 || rIds.Contains(o.RoleId)))
            .ToListAsync(cancellationToken);

        var dailyEntries = new List<AttendanceDailyDetailDto>();
        for (var d = period.StartDate; d <= period.EndDate; d = d.AddDays(1))
        {
            var dayPunch = attendances.FirstOrDefault(a => a.InTime.HasValue && DateOnly.FromDateTime(a.InTime.Value.DateTime) == d);
            var dayLeave = subLeaves.FirstOrDefault(s => s.LeaveDate == d);
            var dayHoliday = holidays.FirstOrDefault(h => h.Date == d);
            var isOff = CheckIfOffDay(d, offDayRules);

            if (dayPunch != null)
            {
                dailyEntries.Add(new AttendanceDailyDetailDto
                {
                    Date = d,
                    Status = dayPunch.Status == 1 ? "Present" : dayPunch.Status == 2 ? "Late" : dayPunch.Status == 3 ? "HalfDay" : "Present",
                    Remarks = dayPunch.Remark ?? (dayPunch.Status == 2 ? "Late Arrival" : "Present")
                });
            }
            else if (dayLeave != null)
            {
                dailyEntries.Add(new AttendanceDailyDetailDto
                {
                    Date = d,
                    Status = dayLeave.IsHalfDay ? "HalfDay" : "OnLeave",
                    Remarks = dayLeave.LeaveApplication?.Reason ?? "Approved Leave"
                });
            }
            else if (dayHoliday != null)
            {
                dailyEntries.Add(new AttendanceDailyDetailDto
                {
                    Date = d,
                    Status = "Holiday",
                    Remarks = dayHoliday.Name
                });
            }
            else if (isOff)
            {
                dailyEntries.Add(new AttendanceDailyDetailDto
                {
                    Date = d,
                    Status = "WeeklyOff",
                    Remarks = "Scheduled Weekly Off"
                });
            }
            else
            {
                dailyEntries.Add(new AttendanceDailyDetailDto
                {
                    Date = d,
                    Status = "Absent",
                    Remarks = "Unexcused Absence (LOP)"
                });
            }
        }

        var breakdown = new PayrollAttendanceBreakdownDto
        {
            EmployeeId = employeeId,
            EmployeeName = pe.Employee.FullName,
            TotalCalendarDays = pe.CalendarDaysInMonth,
            EligibleDays = pe.EligibleEmploymentDays,
            WorkingDays = pe.WorkingDays,
            PresentDays = pe.PresentDays,
            PaidLeaveDays = pe.PaidLeaveDays,
            HalfDays = pe.HalfDays,
            LOPDays = pe.LOPDays,
            ApprovedOvertimeHours = pe.ApprovedOvertimeHours,
            DailyEntries = dailyEntries
        };

        return Ok(ApiResponseDto<PayrollAttendanceBreakdownDto>.Ok(breakdown));
    }

    /// <summary>
    /// Module 2: Get human-readable step-by-step calculation log.
    /// </summary>
    [HttpGet("{employeeId:int}/derivation")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<PayrollDerivationLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeDerivationLog(int periodId, int employeeId, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var pe = await _context.PayrollEmployees
            .Include(p => p.Employee)
            .Include(p => p.Slices)
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.PayrollPeriodId == periodId && p.EmployeeId == employeeId && p.OrganizationId == orgId, cancellationToken);

        if (pe == null)
            return NotFound(ApiResponseDto<PayrollDerivationLogDto>.Fail("Payroll employee record not found."));

        var steps = new List<DerivationStepDto>();
        int step = 1;

        steps.Add(new DerivationStepDto
        {
            StepNumber = step++,
            StepName = "Base Monthly Gross",
            Description = "Base gross salary from active structure",
            Formula = "Structure.MonthlyGrossSalary",
            ResultValue = pe.BaseMonthlyGross
        });

        if (pe.Slices.Count > 1)
        {
            foreach (var slice in pe.Slices)
            {
                steps.Add(new DerivationStepDto
                {
                    StepNumber = step++,
                    StepName = $"Salary Slice V{slice.SalaryStructureVersion}",
                    Description = $"{slice.SliceNotes} ({slice.SliceStartDate:dd-MMM} to {slice.SliceEndDate:dd-MMM})",
                    Formula = $"MonthlyGross * ({slice.EligibleDaysInSlice}/{slice.TotalCalendarDaysInSlice})",
                    ResultValue = slice.ProratedGrossInSlice
                });
            }
        }

        steps.Add(new DerivationStepDto
        {
            StepNumber = step++,
            StepName = "Prorated Gross",
            Description = "Prorated gross pay based on active employment calendar days",
            Formula = "Sum(Slices.ProratedGross)",
            ResultValue = pe.ProratedGross
        });

        if (pe.LOPDays > 0)
        {
            steps.Add(new DerivationStepDto
            {
                StepNumber = step++,
                StepName = "Loss of Pay (LOP) Deduction",
                Description = $"Deduction for {pe.LOPDays} unpaid absence day(s)",
                Formula = $"ProratedGross / Divisor * {pe.LOPDays}",
                ResultValue = pe.LOPDeduction
            });
        }

        if (pe.OvertimePay > 0)
        {
            steps.Add(new DerivationStepDto
            {
                StepNumber = step++,
                StepName = "Overtime Earnings",
                Description = $"Payment for {pe.ApprovedOvertimeHours} approved overtime hour(s)",
                Formula = $"HourlyRate * OTMultiplier * {pe.ApprovedOvertimeHours}",
                ResultValue = pe.OvertimePay
            });
        }

        foreach (var item in pe.Items.OrderBy(i => i.CalculationOrder))
        {
            steps.Add(new DerivationStepDto
            {
                StepNumber = step++,
                StepName = item.ComponentName,
                Description = item.CalculationNotes,
                Formula = item.CalculationFormula,
                ResultValue = item.FinalAmount
            });
        }

        steps.Add(new DerivationStepDto
        {
            StepNumber = step++,
            StepName = "Total Gross Earnings",
            Description = "Sum of all earning items including overtime and allowances",
            Formula = "Sum(Earnings)",
            ResultValue = pe.GrossEarnings
        });

        steps.Add(new DerivationStepDto
        {
            StepNumber = step++,
            StepName = "Total Deductions",
            Description = "Sum of all statutory (PF, ESI, PT, TDS) and other deductions",
            Formula = "Sum(Deductions)",
            ResultValue = pe.TotalDeductions
        });

        steps.Add(new DerivationStepDto
        {
            StepNumber = step++,
            StepName = "Net Payable Salary",
            Description = "Final take-home pay disbursed to employee bank account",
            Formula = "GrossEarnings - TotalDeductions",
            ResultValue = pe.NetPay
        });

        var log = new PayrollDerivationLogDto
        {
            EmployeeId = employeeId,
            EmployeeName = pe.Employee.FullName,
            Steps = steps
        };

        return Ok(ApiResponseDto<PayrollDerivationLogDto>.Ok(log));
    }

    /// <summary>
    /// Module 2: Recalculate salary and statutory derivations for single employee.
    /// </summary>
    [HttpPost("{employeeId:int}/recalculate")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecalculateEmployee(int periodId, int employeeId, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var period = await _context.PayrollPeriods.FirstOrDefaultAsync(p => p.Id == periodId && p.OrganizationId == orgId, cancellationToken);
        if (period == null)
            return NotFound(ApiResponseDto<bool>.Fail("Payroll period not found."));

        if (period.Status == PayrollPeriodStatus.Locked || period.Status == PayrollPeriodStatus.Approved)
            return BadRequest(ApiResponseDto<bool>.Fail("Cannot recalculate employee for approved or locked period."));

        await _calculationEngine.CalculateEmployeePayrollAsync(periodId, employeeId, cancellationToken);
        return Ok(ApiResponseDto<bool>.Ok(true, "Employee payroll recalculated successfully."));
    }

    private static PayrollItemDto MapItemDto(PayrollItem i) => new()
    {
        Id = i.Id,
        ComponentCode = i.ComponentCode,
        ComponentName = i.ComponentName,
        ComponentType = i.ComponentType,
        CalculationOrder = i.CalculationOrder,
        CalculationType = i.CalculationType,
        CalculationBase = i.CalculationBase,
        CalculationRate = i.CalculationRate,
        OriginalAmount = i.OriginalAmount,
        ProratedAmount = i.ProratedAmount,
        FinalAmount = i.FinalAmount,
        CalculationFormula = i.CalculationFormula,
        CalculationNotes = i.CalculationNotes
    };

    private static bool CheckIfOffDay(DateOnly date, List<OffDay> offDayRules)
    {
        var dayName = date.DayOfWeek.ToString();
        var weekNumber = ((date.Day - 1) / 7) + 1;

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
            return date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday;
        }

        return false;
    }
}
