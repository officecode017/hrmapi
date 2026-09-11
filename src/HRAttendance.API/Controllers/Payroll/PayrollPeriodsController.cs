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

namespace HRAttendance.API.Controllers.Payroll;

[ApiController]
[Route("api/payroll/periods")]
[Authorize]
public class PayrollPeriodsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPayrollCalculationEngine _calculationEngine;
    private readonly IPayrollLifecycleService _lifecycleService;
    private readonly IPayrollPaymentService _paymentService;
    private readonly IBankExportService _bankExportService;

    public PayrollPeriodsController(
        ApplicationDbContext context,
        IPayrollCalculationEngine calculationEngine,
        IPayrollLifecycleService lifecycleService,
        IPayrollPaymentService paymentService,
        IBankExportService bankExportService)
    {
        _context = context;
        _calculationEngine = calculationEngine;
        _lifecycleService = lifecycleService;
        _paymentService = paymentService;
        _bankExportService = bankExportService;
    }

    /// <summary>
    /// Module 1: Get paginated payroll periods with optional status and year/month filters.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<PagedResponseDto<PayrollPeriodDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPeriods([FromQuery] PeriodQueryParameters parameters, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var query = _context.PayrollPeriods
            .Include(p => p.FinancialYear)
            .Where(p => p.OrganizationId == orgId);

        if (parameters.Year.HasValue)
            query = query.Where(p => p.Year == parameters.Year.Value);

        if (parameters.Month.HasValue)
            query = query.Where(p => p.Month == parameters.Month.Value);

        if (parameters.Status.HasValue)
            query = query.Where(p => p.Status == parameters.Status.Value);

        if (parameters.RunType.HasValue)
            query = query.Where(p => p.RunType == parameters.RunType.Value);

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            query = query.Where(p => p.Description.Contains(parameters.SearchTerm));

        var totalCount = await query.CountAsync(cancellationToken);

        var pagedEntities = await query
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ThenByDescending(p => p.SequenceNumber)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        var items = pagedEntities
            .Select(p => new PayrollPeriodDto
            {
                Id = p.Id,
                OrganizationId = p.OrganizationId,
                FinancialYearId = p.FinancialYearId,
                YearCode = p.FinancialYear != null ? p.FinancialYear.YearCode : string.Empty,
                Month = p.Month,
                Year = p.Year,
                RunType = p.RunType,
                SequenceNumber = p.SequenceNumber,
                Description = p.Description,
                StartDate = p.StartDate.Year > 1 ? p.StartDate : new DateOnly(p.Year, p.Month, 1),
                EndDate = p.EndDate.Year > 1 ? p.EndDate : new DateOnly(p.Year, p.Month, DateTime.DaysInMonth(p.Year, p.Month)),
                Status = p.Status,
                TotalGrossPay = p.TotalGrossPay,
                TotalDeductions = p.TotalDeductions,
                TotalEmployerContributions = p.TotalEmployerContributions,
                TotalNetPay = p.TotalNetPay,
                TotalEmployeesProcessed = p.TotalEmployeesProcessed,
                TotalExceptionsCount = p.TotalExceptionsCount,
                CalculatedAt = p.CalculatedAt,
                ApprovedBy = p.ApprovedBy,
                ApprovedAt = p.ApprovedAt,
                LockedBy = p.LockedBy,
                LockedAt = p.LockedAt
            })
            .ToList();

        var paged = new PagedResponseDto<PayrollPeriodDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };

        return Ok(ApiResponseDto<PagedResponseDto<PayrollPeriodDto>>.Ok(paged));
    }

    /// <summary>
    /// Module 1: Create a new draft period (Regular or Off-Cycle).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<PayrollPeriodDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePeriod([FromBody] CreatePayrollPeriodDto request, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var empId = User.TryGetEmployeeId() ?? 0;

        // Verify FinancialYear exists
        var fy = await _context.FinancialYears.FirstOrDefaultAsync(f => f.Id == request.FinancialYearId && f.OrganizationId == orgId, cancellationToken);
        if (fy == null)
            return BadRequest(ApiResponseDto<PayrollPeriodDto>.Fail("Specified financial year was not found."));

        // Check if an active period for same org, year, month, run type, sequence already exists
        var exists = await _context.PayrollPeriods.AnyAsync(p =>
            p.OrganizationId == orgId &&
            p.Year == request.Year &&
            p.Month == request.Month &&
            p.RunType == request.RunType &&
            p.SequenceNumber == request.SequenceNumber, cancellationToken);

        if (exists)
            return BadRequest(ApiResponseDto<PayrollPeriodDto>.Fail($"A payroll period for {request.Year}-{request.Month:D2} ({request.RunType} #{request.SequenceNumber}) already exists."));

        // Clean up any historical soft-deleted phantom period with same keys to guarantee clean insertion
        await _context.PayrollPeriods
            .IgnoreQueryFilters()
            .Where(p => p.OrganizationId == orgId &&
                        p.Year == request.Year &&
                        p.Month == request.Month &&
                        p.RunType == request.RunType &&
                        p.SequenceNumber == request.SequenceNumber &&
                        p.IsDeleted)
            .ExecuteDeleteAsync(cancellationToken);

        var startDate = request.StartDate.Year > 1
            ? request.StartDate
            : new DateOnly(request.Year, request.Month, 1);
        var endDate = request.EndDate.Year > 1
            ? request.EndDate
            : new DateOnly(request.Year, request.Month, DateTime.DaysInMonth(request.Year, request.Month));

        var period = new PayrollPeriod
        {
            OrganizationId = orgId,
            FinancialYearId = request.FinancialYearId,
            Month = request.Month,
            Year = request.Year,
            RunType = request.RunType,
            SequenceNumber = request.SequenceNumber,
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? $"Payroll for {request.Year}-{request.Month:D2} ({request.RunType})"
                : request.Description,
            StartDate = startDate,
            EndDate = endDate,
            Status = PayrollPeriodStatus.Draft,
            CreatedBy = empId
        };

        _context.PayrollPeriods.Add(period);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new PayrollPeriodDto
        {
            Id = period.Id,
            OrganizationId = period.OrganizationId,
            FinancialYearId = period.FinancialYearId,
            YearCode = fy.YearCode,
            Month = period.Month,
            Year = period.Year,
            RunType = period.RunType,
            SequenceNumber = period.SequenceNumber,
            Description = period.Description,
            StartDate = period.StartDate,
            EndDate = period.EndDate,
            Status = period.Status
        };

        return CreatedAtAction(nameof(GetPeriodById), new { id = period.Id }, ApiResponseDto<PayrollPeriodDto>.Ok(dto, "Payroll period created successfully."));
    }

    /// <summary>
    /// Module 1: Get period details and audit stamps.
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<PayrollPeriodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPeriodById(int id, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var period = await _context.PayrollPeriods
            .Include(p => p.FinancialYear)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrganizationId == orgId, cancellationToken);

        if (period == null)
            return NotFound(ApiResponseDto<PayrollPeriodDto>.Fail("Payroll period not found."));

        var dto = new PayrollPeriodDto
        {
            Id = period.Id,
            OrganizationId = period.OrganizationId,
            FinancialYearId = period.FinancialYearId,
            YearCode = period.FinancialYear != null ? period.FinancialYear.YearCode : string.Empty,
            Month = period.Month,
            Year = period.Year,
            RunType = period.RunType,
            SequenceNumber = period.SequenceNumber,
            Description = period.Description,
            StartDate = period.StartDate.Year > 1 ? period.StartDate : new DateOnly(period.Year, period.Month, 1),
            EndDate = period.EndDate.Year > 1 ? period.EndDate : new DateOnly(period.Year, period.Month, DateTime.DaysInMonth(period.Year, period.Month)),
            Status = period.Status,
            TotalGrossPay = period.TotalGrossPay,
            TotalDeductions = period.TotalDeductions,
            TotalEmployerContributions = period.TotalEmployerContributions,
            TotalNetPay = period.TotalNetPay,
            TotalEmployeesProcessed = period.TotalEmployeesProcessed,
            TotalExceptionsCount = period.TotalExceptionsCount,
            CalculatedAt = period.CalculatedAt,
            ApprovedBy = period.ApprovedBy,
            ApprovedAt = period.ApprovedAt,
            LockedBy = period.LockedBy,
            LockedAt = period.LockedAt
        };

        return Ok(ApiResponseDto<PayrollPeriodDto>.Ok(dto));
    }

    /// <summary>
    /// Module 1: Get aggregated totals, department breakdown, and exception breakdown.
    /// </summary>
    [HttpGet("{id:int}/summary")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<PayrollPeriodSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPeriodSummary(int id, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var period = await _context.PayrollPeriods
            .Include(p => p.Employees)
                .ThenInclude(e => e.Employee)
                    .ThenInclude(emp => emp.ProfessionalDetails)
                        .ThenInclude(pd => pd!.Department)
            .Include(p => p.Exceptions)
            .FirstOrDefaultAsync(p => p.Id == id && p.OrganizationId == orgId, cancellationToken);

        if (period == null)
            return NotFound(ApiResponseDto<PayrollPeriodSummaryDto>.Fail("Payroll period not found."));

        var deptGroups = period.Employees
            .GroupBy(e => new 
            { 
                Id = e.Employee.ProfessionalDetails?.DepartmentId ?? 0, 
                Name = e.Employee.ProfessionalDetails?.Department?.Name ?? "Unassigned" 
            })
            .Select(g => new DepartmentPayrollSummaryDto
            {
                DepartmentId = g.Key.Id,
                DepartmentName = g.Key.Name,
                EmployeeCount = g.Count(),
                TotalGrossPay = g.Sum(x => x.GrossEarnings),
                TotalNetPay = g.Sum(x => x.NetPay)
            })
            .ToList();

        var summary = new PayrollPeriodSummaryDto
        {
            PeriodId = period.Id,
            PeriodName = period.Description,
            Status = period.Status,
            RunType = period.RunType,
            StartDate = period.StartDate.Year > 1 ? period.StartDate : new DateOnly(period.Year, period.Month, 1),
            EndDate = period.EndDate.Year > 1 ? period.EndDate : new DateOnly(period.Year, period.Month, DateTime.DaysInMonth(period.Year, period.Month)),
            TotalGrossPay = period.TotalGrossPay,
            TotalDeductions = period.TotalDeductions,
            TotalEmployerContributions = period.TotalEmployerContributions,
            TotalNetPay = period.TotalNetPay,
            TotalEmployees = period.Employees.Count,
            PendingEmployees = period.Employees.Count(e => e.Status == PayrollEmployeeStatus.Pending),
            ProcessedEmployees = period.Employees.Count(e => e.Status == PayrollEmployeeStatus.Calculated || e.Status == PayrollEmployeeStatus.Overridden),
            ErrorEmployees = period.Employees.Count(e => e.Status == PayrollEmployeeStatus.Excluded),
            CriticalExceptionsCount = period.Exceptions.Count(x => x.Severity == ExceptionSeverity.Critical && !x.IsResolved),
            WarningExceptionsCount = period.Exceptions.Count(x => x.Severity == ExceptionSeverity.Warning && !x.IsResolved),
            DepartmentBreakdown = deptGroups
        };

        return Ok(ApiResponseDto<PayrollPeriodSummaryDto>.Ok(summary));
    }

    /// <summary>
    /// Module 1: Run batch calculation across all eligible employees.
    /// </summary>
    [HttpPost("{id:int}/calculate")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<PayrollCalculationBatchResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CalculatePeriod(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _calculationEngine.CalculatePeriodBatchAsync(id, cancellationToken);
            return Ok(ApiResponseDto<PayrollCalculationBatchResult>.Ok(result, "Payroll calculated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<PayrollCalculationBatchResult>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Module 1: Re-run calculation (increments CalculationVersion).
    /// </summary>
    [HttpPost("{id:int}/recalculate")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<PayrollCalculationBatchResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecalculatePeriod(int id, CancellationToken cancellationToken)
    {
        // Recalculate period is functionally the calculation engine run on an already-calculated period
        return await CalculatePeriod(id, cancellationToken);
    }

    /// <summary>
    /// Module 1: Move period from Calculated to UnderReview.
    /// </summary>
    [HttpPost("{id:int}/submit-review")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitForReview(int id, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var empId = User.TryGetEmployeeId() ?? 0;

        try
        {
            await _lifecycleService.SubmitForReviewAsync(id, empId, cancellationToken);
            return Ok(ApiResponseDto<bool>.Ok(true, "Payroll submitted for review successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<bool>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Module 1: Executive approval stamp (sets ApprovedBy/At).
    /// </summary>
    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApprovePeriod(int id, CancellationToken cancellationToken)
    {
        var empId = User.TryGetEmployeeId() ?? 0;

        try
        {
            await _lifecycleService.ApproveAsync(id, empId, cancellationToken);
            return Ok(ApiResponseDto<bool>.Ok(true, "Payroll approved successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<bool>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Module 1: Financial Freeze: generates immutable payslips & locks.
    /// </summary>
    [HttpPost("{id:int}/lock")]
    [Authorize(Roles = ApplicationRoles.SuperAdmin)]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LockPeriod(int id, CancellationToken cancellationToken)
    {
        var empId = User.TryGetEmployeeId() ?? 0;

        try
        {
            await _lifecycleService.LockAsync(id, empId, cancellationToken);
            return Ok(ApiResponseDto<bool>.Ok(true, "Payroll period locked and finalized. Immutable payslips generated."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<bool>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Module 1: Controlled unlock from Approved/UnderReview back to Draft.
    /// </summary>
    [HttpPost("{id:int}/reopen")]
    [Authorize(Roles = ApplicationRoles.SuperAdmin)]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReopenPeriod(int id, [FromBody] string reason, CancellationToken cancellationToken)
    {
        var empId = User.TryGetEmployeeId() ?? 0;

        try
        {
            await _lifecycleService.ReopenAsync(id, empId, cancellationToken);
            return Ok(ApiResponseDto<bool>.Ok(true, "Payroll period reopened to Draft."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<bool>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Module 1: Cancel an un-locked payroll period.
    /// </summary>
    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelPeriod(int id, [FromBody] string reason, CancellationToken cancellationToken)
    {
        var empId = User.TryGetEmployeeId() ?? 0;

        try
        {
            await _lifecycleService.CancelAsync(id, empId, cancellationToken);
            return Ok(ApiResponseDto<bool>.Ok(true, "Payroll period cancelled."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<bool>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Module 1: Reset calculated financials and remove generated child records back to pristine Draft.
    /// </summary>
    [HttpPost("{id:int}/reset-financials")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetFinancials(int id, CancellationToken cancellationToken)
    {
        var empId = User.TryGetEmployeeId() ?? 0;

        try
        {
            await _lifecycleService.ResetFinancialsAsync(id, empId, cancellationToken);
            return Ok(ApiResponseDto<bool>.Ok(true, "Payroll financials and generated child records reset to Draft."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<bool>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Module 1: Delete a payroll period and all its child calculations.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeletePeriod(int id, CancellationToken cancellationToken)
    {
        var empId = User.TryGetEmployeeId() ?? 0;

        try
        {
            await _lifecycleService.DeletePeriodAsync(id, empId, cancellationToken);
            return Ok(ApiResponseDto<bool>.Ok(true, "Payroll period deleted successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<bool>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Module 5 nested: List all bonuses, advances, and deductions for period.
    /// </summary>
    [HttpGet("{periodId:int}/adjustments")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    public async Task<IActionResult> GetPeriodAdjustments(int periodId, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var adjustments = await _context.PayrollAdjustments
            .Where(a => a.PayrollPeriodId == periodId && a.OrganizationId == orgId)
            .ToListAsync(cancellationToken);

        var empIds = adjustments.Select(a => a.EmployeeId).Distinct().ToList();
        var emps = await _context.Employees
            .Where(e => empIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => new { FullName = (e.FirstName ?? "") + " " + (e.LastName ?? ""), e.EmployeeCode }, cancellationToken);

        var dtos = adjustments.Select(a => new PayrollAdjustmentDto
        {
            Id = a.Id,
            AdjustmentNumber = a.AdjustmentNumber,
            OrganizationId = a.OrganizationId,
            EmployeeId = a.EmployeeId,
            EmployeeName = emps.TryGetValue(a.EmployeeId, out var emp) ? emp.FullName : string.Empty,
            EmployeeCode = emp != null ? emp.EmployeeCode : string.Empty,
            PayrollPeriodId = a.PayrollPeriodId,
            Type = a.Type,
            Direction = a.Direction,
            Amount = a.Amount,
            Reason = a.Reason,
            Status = a.Status,
            ApprovedBy = a.ApprovedBy,
            ApprovedAt = a.ApprovedAt,
            RejectedBy = a.RejectedBy,
            RejectedAt = a.RejectedAt,
            RejectionReason = a.RejectionReason
        }).ToList();

        return Ok(ApiResponseDto<List<PayrollAdjustmentDto>>.Ok(dtos));
    }

    /// <summary>
    /// Module 6 nested: List all arrears scheduled to pay out in this period.
    /// </summary>
    [HttpGet("{periodId:int}/arrears")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    public async Task<IActionResult> GetPeriodArrears(int periodId, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var arrears = await _context.PayrollArrears
            .Where(a => a.TargetPayrollPeriodId == periodId && a.OrganizationId == orgId)
            .ToListAsync(cancellationToken);

        var empIds = arrears.Select(a => a.EmployeeId).Distinct().ToList();
        var emps = await _context.Employees
            .Where(e => empIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => new { FullName = (e.FirstName ?? "") + " " + (e.LastName ?? ""), e.EmployeeCode }, cancellationToken);

        var periodIds = arrears.Select(a => a.SourcePayrollPeriodId).Distinct().ToList();
        var periods = await _context.PayrollPeriods
            .Where(p => periodIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Description, cancellationToken);

        var dtos = arrears.Select(a => new PayrollArrearDto
        {
            Id = a.Id,
            ArrearNumber = a.ArrearNumber,
            OrganizationId = a.OrganizationId,
            EmployeeId = a.EmployeeId,
            EmployeeName = emps.TryGetValue(a.EmployeeId, out var emp) ? emp.FullName : string.Empty,
            EmployeeCode = emp != null ? emp.EmployeeCode : string.Empty,
            SourcePayrollPeriodId = a.SourcePayrollPeriodId,
            SourcePeriodName = periods.TryGetValue(a.SourcePayrollPeriodId, out var pName) ? pName : string.Empty,
            TargetPayrollPeriodId = a.TargetPayrollPeriodId,
            TargetPeriodName = string.Empty,
            ComponentCode = a.ComponentCode,
            OriginalAmount = a.OriginalAmount,
            CorrectAmount = a.CorrectAmount,
            DifferenceAmount = a.DifferenceAmount,
            Reason = a.Reason,
            Status = a.Status
        }).ToList();

        return Ok(ApiResponseDto<List<PayrollArrearDto>>.Ok(dtos));
    }

    /// <summary>
    /// Module 7 nested: List all pre-flight exceptions grouped by severity.
    /// </summary>
    [HttpGet("{periodId:int}/exceptions")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    public async Task<IActionResult> GetPeriodExceptions(int periodId, CancellationToken cancellationToken)
    {
        var exceptions = await _context.PayrollExceptions
            .Where(x => x.PayrollPeriodId == periodId)
            .ToListAsync(cancellationToken);

        var empIds = exceptions.Select(x => x.EmployeeId).Distinct().ToList();
        var emps = await _context.Employees
            .Where(e => empIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => new { FullName = (e.FirstName ?? "") + " " + (e.LastName ?? ""), e.EmployeeCode }, cancellationToken);

        var dtos = exceptions.Select(x => new PayrollExceptionDto
        {
            Id = x.Id,
            PayrollPeriodId = x.PayrollPeriodId,
            EmployeeId = x.EmployeeId,
            EmployeeName = emps.TryGetValue(x.EmployeeId, out var emp) ? emp.FullName : string.Empty,
            EmployeeCode = emp != null ? emp.EmployeeCode : string.Empty,
            Severity = x.Severity,
            ErrorCode = x.ErrorCode,
            Message = x.Message,
            IsResolved = x.IsResolved,
            ResolvedBy = x.ResolvedBy,
            ResolvedAt = x.ResolvedAt,
            ResolutionNotes = x.ResolutionNotes
        }).ToList();

        return Ok(ApiResponseDto<List<PayrollExceptionDto>>.Ok(dtos));
    }

    /// <summary>
    /// Module 8 nested: List all generated payslips for period.
    /// </summary>
    [HttpGet("{periodId:int}/payslips")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    public async Task<IActionResult> GetPeriodPayslips(int periodId, CancellationToken cancellationToken)
    {
        var payslips = await _context.Payslips
            .Include(p => p.PayrollEmployee)
                .ThenInclude(pe => pe.Employee)
            .Where(p => p.PayrollEmployee.PayrollPeriodId == periodId)
            .ToListAsync(cancellationToken);

        var dtos = payslips.Select(p => new MyPayslipSummaryDto
        {
            Id = p.Id,
            PayslipNumber = p.PayslipNumber,
            EmployeeName = (p.PayrollEmployee.Employee.FirstName ?? "") + " " + (p.PayrollEmployee.Employee.LastName ?? ""),
            EmployeeCode = p.PayrollEmployee.Employee.EmployeeCode ?? string.Empty,
            Month = p.PayrollEmployee.Period != null ? p.PayrollEmployee.Period.Month : 0,
            Year = p.PayrollEmployee.Period != null ? p.PayrollEmployee.Period.Year : 0,
            PeriodDescription = p.PayrollEmployee.Period?.Description ?? string.Empty,
            GrossEarnings = p.PayrollEmployee.GrossEarnings,
            TotalDeductions = p.PayrollEmployee.TotalDeductions,
            NetPay = p.PayrollEmployee.NetPay,
            Status = p.Status,
            GeneratedAt = p.GeneratedAt
        }).ToList();

        return Ok(ApiResponseDto<List<MyPayslipSummaryDto>>.Ok(dtos));
    }

    /// <summary>
    /// Module 8 nested: Bulk publish all locked payslips in period.
    /// </summary>
    [HttpPost("{periodId:int}/payslips/publish-all")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    public async Task<IActionResult> PublishAllPayslips(int periodId, CancellationToken cancellationToken)
    {
        var payslips = await _context.Payslips
            .Include(p => p.PayrollEmployee)
            .Where(p => p.PayrollEmployee.PayrollPeriodId == periodId && p.Status == PayslipStatus.Generated)
            .ToListAsync(cancellationToken);

        foreach (var p in payslips)
        {
            p.Status = PayslipStatus.Published;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponseDto<int>.Ok(payslips.Count, $"{payslips.Count} payslips published successfully."));
    }

    /// <summary>
    /// Module 9 nested: List payment ledger records with attempt counters.
    /// </summary>
    [HttpGet("{periodId:int}/payments")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    public async Task<IActionResult> GetPeriodPayments(int periodId, CancellationToken cancellationToken)
    {
        var payments = await _context.PayrollPayments
            .Include(p => p.PayrollEmployee)
                .ThenInclude(pe => pe.Employee)
            .Where(p => p.PayrollEmployee.PayrollPeriodId == periodId)
            .ToListAsync(cancellationToken);

        var dtos = payments.Select(PayrollPaymentDto.FromEntity).ToList();

        return Ok(ApiResponseDto<List<PayrollPaymentDto>>.Ok(dtos));
    }

    /// <summary>
    /// Module 9: Initiate bulk payment cycle for locked period (Idempotent).
    /// </summary>
    [HttpPost("{periodId:int}/disburse")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    public async Task<IActionResult> DisbursePeriod(int periodId, [FromBody] DisbursePeriodDto request, CancellationToken cancellationToken)
    {
        var empId = User.TryGetEmployeeId() ?? 0;

        try
        {
            var result = await _paymentService.InitiateDisbursementAsync(
                periodId,
                request.IdempotencyKey,
                empId,
                cancellationToken);

            return Ok(ApiResponseDto<int>.Ok(result.TotalPaymentsInitiated, $"{result.TotalPaymentsInitiated} disbursement payment records created/retrieved."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<int>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Module 10 nested: List historical bank export batches for period.
    /// </summary>
    [HttpGet("{periodId:int}/bank-exports")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    public async Task<IActionResult> GetPeriodBankExports(int periodId, CancellationToken cancellationToken)
    {
        var batches = await _bankExportService.GetExportBatchesForPeriodAsync(periodId, cancellationToken);

        var dtos = batches.Select(b => new BankExportBatchDto
        {
            Id = b.Id,
            OrganizationId = b.OrganizationId,
            PayrollPeriodId = b.PayrollPeriodId,
            BatchNumber = b.BatchNumber,
            Format = b.Format,
            TotalRecords = b.TotalRecords,
            TotalAmount = b.TotalAmount,
            StoragePath = b.StoragePath,
            ExportedAt = b.ExportedAt,
            ExportedBy = b.ExportedBy
        }).ToList();

        return Ok(ApiResponseDto<List<BankExportBatchDto>>.Ok(dtos));
    }
}
