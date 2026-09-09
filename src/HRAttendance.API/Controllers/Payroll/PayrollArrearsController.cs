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
[Route("api/payroll/arrears")]
[Authorize]
public class PayrollArrearsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPayrollArrearService _arrearService;

    public PayrollArrearsController(
        ApplicationDbContext context,
        IPayrollArrearService arrearService)
    {
        _context = context;
        _arrearService = arrearService;
    }

    /// <summary>
    /// Module 6: List arrears with optional target period, employee, and status filters.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<List<PayrollArrearDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetArrears(
        [FromQuery] int? targetPeriodId,
        [FromQuery] int? employeeId,
        [FromQuery] ArrearStatus? status,
        CancellationToken cancellationToken = default)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var query = _context.PayrollArrears
            .Where(a => a.OrganizationId == orgId);

        if (targetPeriodId.HasValue && targetPeriodId.Value > 0)
            query = query.Where(a => a.TargetPayrollPeriodId == targetPeriodId.Value);

        if (employeeId.HasValue && employeeId.Value > 0)
            query = query.Where(a => a.EmployeeId == employeeId.Value);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        var arrears = await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        var empIds = arrears.Select(a => a.EmployeeId).Distinct().ToList();
        var employeeMap = await _context.Employees
            .Where(e => empIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => new { FullName = (e.FirstName ?? "") + " " + (e.LastName ?? ""), e.EmployeeCode }, cancellationToken);

        var periodIds = arrears.Select(a => a.SourcePayrollPeriodId)
            .Concat(arrears.Select(a => a.TargetPayrollPeriodId))
            .Distinct()
            .ToList();

        var periodMap = await _context.PayrollPeriods
            .Where(p => periodIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Description, cancellationToken);

        var dtos = arrears.Select(a =>
        {
            employeeMap.TryGetValue(a.EmployeeId, out var emp);
            periodMap.TryGetValue(a.SourcePayrollPeriodId, out var srcName);
            periodMap.TryGetValue(a.TargetPayrollPeriodId, out var tgtName);

            return new PayrollArrearDto
            {
                Id = a.Id,
                ArrearNumber = a.ArrearNumber,
                OrganizationId = a.OrganizationId,
                EmployeeId = a.EmployeeId,
                EmployeeName = emp?.FullName ?? string.Empty,
                EmployeeCode = emp?.EmployeeCode ?? string.Empty,
                SourcePayrollPeriodId = a.SourcePayrollPeriodId,
                SourcePeriodName = srcName ?? string.Empty,
                TargetPayrollPeriodId = a.TargetPayrollPeriodId,
                TargetPeriodName = tgtName ?? string.Empty,
                ComponentCode = a.ComponentCode,
                OriginalAmount = a.OriginalAmount,
                CorrectAmount = a.CorrectAmount,
                DifferenceAmount = a.DifferenceAmount,
                Reason = a.Reason,
                Status = a.Status
            };
        }).ToList();

        return Ok(ApiResponseDto<List<PayrollArrearDto>>.Ok(dtos));
    }

    /// <summary>
    /// Module 6: Stage a retroactive arrear for an upcoming period.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<PayrollArrearDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StageArrear([FromBody] CreatePayrollArrearDto request, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var empId = User.TryGetEmployeeId() ?? 0;

        try
        {
            var serviceDto = new HRAttendance.Business.Interfaces.Payroll.CreateArrearDto(
                orgId,
                request.EmployeeId,
                request.SourcePayrollPeriodId,
                request.TargetPayrollPeriodId,
                request.ComponentCode,
                request.OriginalAmount,
                request.CorrectAmount,
                request.Reason);

            var arrear = await _arrearService.CreateArrearAsync(serviceDto, empId, cancellationToken);

            return await GetArrearById(arrear.Id, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<PayrollArrearDto>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Module 6: Get arrear details, differences, and source period.
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<PayrollArrearDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetArrearById(int id, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var arrear = await _context.PayrollArrears
            .FirstOrDefaultAsync(a => a.Id == id && a.OrganizationId == orgId, cancellationToken);

        if (arrear == null)
            return NotFound(ApiResponseDto<PayrollArrearDto>.Fail("Payroll arrear not found."));

        var employee = await _context.Employees
            .Where(e => e.Id == arrear.EmployeeId)
            .Select(e => new { FullName = (e.FirstName ?? "") + " " + (e.LastName ?? ""), e.EmployeeCode })
            .FirstOrDefaultAsync(cancellationToken);

        var sourcePeriod = await _context.PayrollPeriods
            .Where(p => p.Id == arrear.SourcePayrollPeriodId)
            .Select(p => p.Description)
            .FirstOrDefaultAsync(cancellationToken);

        var targetPeriod = await _context.PayrollPeriods
            .Where(p => p.Id == arrear.TargetPayrollPeriodId)
            .Select(p => p.Description)
            .FirstOrDefaultAsync(cancellationToken);

        var dto = new PayrollArrearDto
        {
            Id = arrear.Id,
            ArrearNumber = arrear.ArrearNumber,
            OrganizationId = arrear.OrganizationId,
            EmployeeId = arrear.EmployeeId,
            EmployeeName = employee?.FullName ?? string.Empty,
            EmployeeCode = employee?.EmployeeCode ?? string.Empty,
            SourcePayrollPeriodId = arrear.SourcePayrollPeriodId,
            SourcePeriodName = sourcePeriod ?? string.Empty,
            TargetPayrollPeriodId = arrear.TargetPayrollPeriodId,
            TargetPeriodName = targetPeriod ?? string.Empty,
            ComponentCode = arrear.ComponentCode,
            OriginalAmount = arrear.OriginalAmount,
            CorrectAmount = arrear.CorrectAmount,
            DifferenceAmount = arrear.DifferenceAmount,
            Reason = arrear.Reason,
            Status = arrear.Status
        };

        return Ok(ApiResponseDto<PayrollArrearDto>.Ok(dto));
    }

    /// <summary>
    /// Module 6: Recalculate difference from source period.
    /// </summary>
    [HttpPost("{id:int}/calculate")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<PayrollArrearDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CalculateDifference(int id, CancellationToken cancellationToken)
    {
        try
        {
            var diff = await _arrearService.CalculateArrearDifferenceAsync(id, cancellationToken);
            return await GetArrearById(id, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<PayrollArrearDto>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Module 6: Mark arrear as applied.
    /// </summary>
    [HttpPost("{id:int}/apply")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApplyArrear(int id, CancellationToken cancellationToken)
    {
        var empId = User.TryGetEmployeeId() ?? 0;

        try
        {
            await _arrearService.ApplyArrearAsync(id, empId, cancellationToken);
            return Ok(ApiResponseDto<bool>.Ok(true, "Arrear marked as applied."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<bool>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Module 6: Cancel staged arrear.
    /// </summary>
    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelArrear(int id, CancellationToken cancellationToken)
    {
        var empId = User.TryGetEmployeeId() ?? 0;

        try
        {
            await _arrearService.CancelArrearAsync(id, empId, cancellationToken);
            return Ok(ApiResponseDto<bool>.Ok(true, "Arrear cancelled."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<bool>.Fail(ex.Message));
        }
    }
}
