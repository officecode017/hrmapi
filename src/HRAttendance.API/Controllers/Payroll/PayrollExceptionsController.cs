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
[Authorize]
public class PayrollExceptionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPayrollValidationService _validationService;

    public PayrollExceptionsController(
        ApplicationDbContext context,
        IPayrollValidationService validationService)
    {
        _context = context;
        _validationService = validationService;
    }

    /// <summary>
    /// Module 7: List exceptions across periods or for a specific period with severity and resolution filters.
    /// </summary>
    [HttpGet("api/payroll/exceptions")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<List<PayrollExceptionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllExceptions(
        [FromQuery] int? periodId,
        [FromQuery] ExceptionSeverity? severity,
        [FromQuery] bool? isResolved,
        CancellationToken cancellationToken = default)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var query = _context.PayrollExceptions
            .Include(x => x.PayrollPeriod)
            .Where(x => x.PayrollPeriod.OrganizationId == orgId);

        if (periodId.HasValue && periodId.Value > 0)
            query = query.Where(x => x.PayrollPeriodId == periodId.Value);

        if (severity.HasValue)
            query = query.Where(x => x.Severity == severity.Value);

        if (isResolved.HasValue)
            query = query.Where(x => x.IsResolved == isResolved.Value);

        var exceptions = await query
            .OrderByDescending(x => x.Severity)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var empIds = exceptions.Select(x => x.EmployeeId).Distinct().ToList();
        var employeeMap = await _context.Employees
            .Where(e => empIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => new { FullName = (e.FirstName ?? "") + " " + (e.LastName ?? ""), e.EmployeeCode }, cancellationToken);

        var dtos = exceptions.Select(ex =>
        {
            employeeMap.TryGetValue(ex.EmployeeId, out var emp);

            return new PayrollExceptionDto
            {
                Id = ex.Id,
                PayrollPeriodId = ex.PayrollPeriodId,
                EmployeeId = ex.EmployeeId,
                EmployeeName = emp?.FullName ?? string.Empty,
                EmployeeCode = emp?.EmployeeCode ?? string.Empty,
                Severity = ex.Severity,
                ErrorCode = ex.ErrorCode,
                Message = ex.Message,
                IsResolved = ex.IsResolved,
                ResolvedBy = ex.ResolvedBy,
                ResolvedAt = ex.ResolvedAt,
                ResolutionNotes = ex.ResolutionNotes
            };
        }).ToList();

        return Ok(ApiResponseDto<List<PayrollExceptionDto>>.Ok(dtos));
    }

    /// <summary>
    /// Module 7: List exceptions for a specific payroll period.
    /// </summary>
    [HttpGet("api/payroll/exceptions/period/{periodId:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<List<PayrollExceptionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPeriodExceptions(int periodId, CancellationToken cancellationToken)
    {
        return await GetAllExceptions(periodId, null, null, cancellationToken);
    }

    /// <summary>
    /// Module 7: Get single exception details and audit state.
    /// </summary>
    [HttpGet("api/payroll/exceptions/{id:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<PayrollExceptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExceptionById(int id, CancellationToken cancellationToken)
    {
        var ex = await _context.PayrollExceptions
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (ex == null)
            return NotFound(ApiResponseDto<PayrollExceptionDto>.Fail("Payroll exception not found."));

        var employee = await _context.Employees
            .Where(e => e.Id == ex.EmployeeId)
            .Select(e => new { e.FullName, e.EmployeeCode })
            .FirstOrDefaultAsync(cancellationToken);

        var dto = new PayrollExceptionDto
        {
            Id = ex.Id,
            PayrollPeriodId = ex.PayrollPeriodId,
            EmployeeId = ex.EmployeeId,
            EmployeeName = employee?.FullName ?? string.Empty,
            EmployeeCode = employee?.EmployeeCode ?? string.Empty,
            Severity = ex.Severity,
            ErrorCode = ex.ErrorCode,
            Message = ex.Message,
            IsResolved = ex.IsResolved,
            ResolvedBy = ex.ResolvedBy,
            ResolvedAt = ex.ResolvedAt,
            ResolutionNotes = ex.ResolutionNotes
        };

        return Ok(ApiResponseDto<PayrollExceptionDto>.Ok(dto));
    }

    /// <summary>
    /// Module 7: Override/resolve a warning exception with notes (via /api/payroll/periods/{periodId}/exceptions/{exceptionId}/resolve).
    /// </summary>
    [HttpPost("api/payroll/periods/{periodId:int}/exceptions/{exceptionId:int}/resolve")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResolveExceptionForPeriod(int periodId, int exceptionId, [FromBody] ResolvePayrollExceptionDto request, CancellationToken cancellationToken)
    {
        return await ResolveInternal(exceptionId, request, cancellationToken);
    }

    /// <summary>
    /// Module 7: Direct resolve by exception ID (via /api/payroll/exceptions/{id}/resolve).
    /// </summary>
    [HttpPost("api/payroll/exceptions/{id:int}/resolve")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResolveException(int id, [FromBody] ResolvePayrollExceptionDto request, CancellationToken cancellationToken)
    {
        return await ResolveInternal(id, request, cancellationToken);
    }

    private async Task<IActionResult> ResolveInternal(int exceptionId, ResolvePayrollExceptionDto request, CancellationToken cancellationToken)
    {
        var empId = User.TryGetEmployeeId() ?? 0;

        try
        {
            await _validationService.ResolveExceptionAsync(exceptionId, empId, request.ResolutionNotes, cancellationToken);
            return Ok(ApiResponseDto<bool>.Ok(true, "Exception resolved successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<bool>.Fail(ex.Message));
        }
    }
}
