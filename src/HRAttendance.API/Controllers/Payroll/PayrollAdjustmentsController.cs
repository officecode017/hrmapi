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
[Route("api/payroll/adjustments")]
[Authorize]
public class PayrollAdjustmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPayrollAdjustmentService _adjustmentService;

    public PayrollAdjustmentsController(
        ApplicationDbContext context,
        IPayrollAdjustmentService adjustmentService)
    {
        _context = context;
        _adjustmentService = adjustmentService;
    }

    /// <summary>
    /// Module 5: List adjustments with optional period, employee, and status filters.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<List<PayrollAdjustmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdjustments(
        [FromQuery] int? payrollPeriodId,
        [FromQuery] int? employeeId,
        [FromQuery] AdjustmentStatus? status,
        CancellationToken cancellationToken = default)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var query = _context.PayrollAdjustments
            .Where(a => a.OrganizationId == orgId);

        if (payrollPeriodId.HasValue && payrollPeriodId.Value > 0)
            query = query.Where(a => a.PayrollPeriodId == payrollPeriodId.Value);

        if (employeeId.HasValue && employeeId.Value > 0)
            query = query.Where(a => a.EmployeeId == employeeId.Value);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        var adjustments = await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        var empIds = adjustments.Select(a => a.EmployeeId).Distinct().ToList();
        var employeeMap = await _context.Employees
            .Where(e => empIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => new { FullName = (e.FirstName ?? "") + " " + (e.LastName ?? ""), e.EmployeeCode }, cancellationToken);

        var dtos = adjustments.Select(adj =>
        {
            employeeMap.TryGetValue(adj.EmployeeId, out var emp);
            return new PayrollAdjustmentDto
            {
                Id = adj.Id,
                AdjustmentNumber = adj.AdjustmentNumber,
                OrganizationId = adj.OrganizationId,
                EmployeeId = adj.EmployeeId,
                EmployeeName = emp?.FullName ?? string.Empty,
                EmployeeCode = emp?.EmployeeCode ?? string.Empty,
                PayrollPeriodId = adj.PayrollPeriodId,
                Type = adj.Type,
                Direction = adj.Direction,
                Amount = adj.Amount,
                Reason = adj.Reason,
                Status = adj.Status,
                ApprovedBy = adj.ApprovedBy,
                ApprovedAt = adj.ApprovedAt,
                RejectedBy = adj.RejectedBy,
                RejectedAt = adj.RejectedAt,
                RejectionReason = adj.RejectionReason
            };
        }).ToList();

        return Ok(ApiResponseDto<List<PayrollAdjustmentDto>>.Ok(dtos));
    }

    /// <summary>
    /// Module 5: Create a new adjustment with justification.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<PayrollAdjustmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAdjustment([FromBody] CreatePayrollAdjustmentDto request, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var empId = User.TryGetEmployeeId() ?? 0;

        try
        {
            var serviceDto = new HRAttendance.Business.Interfaces.Payroll.CreateAdjustmentDto(
                orgId,
                request.EmployeeId,
                request.PayrollPeriodId,
                request.Type,
                request.Direction,
                request.Amount,
                request.Reason);

            var adj = await _adjustmentService.CreateAdjustmentAsync(serviceDto, empId, cancellationToken);

            return await GetAdjustmentById(adj.Id, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<PayrollAdjustmentDto>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Module 5: Get adjustment details, audit log, and status.
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<PayrollAdjustmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAdjustmentById(int id, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var adj = await _context.PayrollAdjustments
            .FirstOrDefaultAsync(a => a.Id == id && a.OrganizationId == orgId, cancellationToken);

        if (adj == null)
            return NotFound(ApiResponseDto<PayrollAdjustmentDto>.Fail("Payroll adjustment not found."));

        var employee = await _context.Employees
            .Where(e => e.Id == adj.EmployeeId)
            .Select(e => new { FullName = (e.FirstName ?? "") + " " + (e.LastName ?? ""), e.EmployeeCode })
            .FirstOrDefaultAsync(cancellationToken);

        var dto = new PayrollAdjustmentDto
        {
            Id = adj.Id,
            AdjustmentNumber = adj.AdjustmentNumber,
            OrganizationId = adj.OrganizationId,
            EmployeeId = adj.EmployeeId,
            EmployeeName = employee?.FullName ?? string.Empty,
            EmployeeCode = employee?.EmployeeCode ?? string.Empty,
            PayrollPeriodId = adj.PayrollPeriodId,
            Type = adj.Type,
            Direction = adj.Direction,
            Amount = adj.Amount,
            Reason = adj.Reason,
            Status = adj.Status,
            ApprovedBy = adj.ApprovedBy,
            ApprovedAt = adj.ApprovedAt,
            RejectedBy = adj.RejectedBy,
            RejectedAt = adj.RejectedAt,
            RejectionReason = adj.RejectionReason
        };

        return Ok(ApiResponseDto<PayrollAdjustmentDto>.Ok(dto));
    }

    /// <summary>
    /// Module 5: Approve a pending adjustment.
    /// </summary>
    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveAdjustment(int id, CancellationToken cancellationToken)
    {
        var empId = User.TryGetEmployeeId() ?? 0;

        try
        {
            await _adjustmentService.ApproveAdjustmentAsync(id, empId, cancellationToken);
            return Ok(ApiResponseDto<bool>.Ok(true, "Adjustment approved successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<bool>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Module 5: Reject adjustment with mandatory reason.
    /// </summary>
    [HttpPost("{id:int}/reject")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RejectAdjustment(int id, [FromBody] RejectAdjustmentDto request, CancellationToken cancellationToken)
    {
        var empId = User.TryGetEmployeeId() ?? 0;

        try
        {
            await _adjustmentService.RejectAdjustmentAsync(id, empId, request.Reason, cancellationToken);
            return Ok(ApiResponseDto<bool>.Ok(true, "Adjustment rejected."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<bool>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Module 5: Cancel an unapplied adjustment.
    /// </summary>
    [HttpPost("{id:int}/cancel")]
    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelAdjustment(int id, CancellationToken cancellationToken)
    {
        var empId = User.TryGetEmployeeId() ?? 0;

        try
        {
            await _adjustmentService.CancelAdjustmentAsync(id, empId, cancellationToken);
            return Ok(ApiResponseDto<bool>.Ok(true, "Adjustment cancelled."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<bool>.Fail(ex.Message));
        }
    }
}
