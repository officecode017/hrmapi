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
public class PayslipsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPayslipPdfGenerator _pdfGenerator;
    private readonly IPayslipProjectionService _projectionService;

    public PayslipsController(
        ApplicationDbContext context,
        IPayslipPdfGenerator pdfGenerator,
        IPayslipProjectionService projectionService)
    {
        _context = context;
        _pdfGenerator = pdfGenerator;
        _projectionService = projectionService;
    }

    /// <summary>
    /// Module 8: Get payslip metadata and itemized presentation table.
    /// </summary>
    [HttpGet("api/payroll/payslips/{id:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin},{ApplicationRoles.Employee}")]
    [ProducesResponseType(typeof(ApiResponseDto<PayslipDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPayslipById(int id, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var currentEmpId = User.GetEmployeeId();

        var payslip = await _context.Payslips
            .Include(p => p.PayrollEmployee)
                .ThenInclude(pe => pe.Employee)
                    .ThenInclude(e => e.ProfessionalDetails)
                        .ThenInclude(pd => pd!.Department)
            .Include(p => p.PayrollEmployee)
                .ThenInclude(pe => pe.Employee)
                    .ThenInclude(e => e.ProfessionalDetails)
                        .ThenInclude(pd => pd!.Designation)
            .Include(p => p.PayrollEmployee)
                .ThenInclude(pe => pe.Period)
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id && p.PayrollEmployee.OrganizationId == orgId, cancellationToken);

        if (payslip == null)
            return NotFound(ApiResponseDto<PayslipDto>.Fail("Payslip not found."));

        // Authorization check: non-admin can only view their own published payslip
        if (!User.IsManagerOrAdmin())
        {
            if (payslip.EmployeeId != currentEmpId)
                return Forbid();

            if (payslip.Status != PayslipStatus.Published)
                return NotFound(ApiResponseDto<PayslipDto>.Fail("Payslip is not yet published."));
        }

        // Record audit view log
        _context.PayslipAccessLogs.Add(new PayslipAccessLog
        {
            PayslipId = payslip.Id,
            AccessedBy = currentEmpId,
            AccessedAt = DateTimeOffset.UtcNow,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            UserAgent = Request.Headers.UserAgent.ToString(),
            ActionType = PayslipActionType.View
        });
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new PayslipDto
        {
            Id = payslip.Id,
            PayrollEmployeeId = payslip.PayrollEmployeeId,
            EmployeeId = payslip.EmployeeId,
            EmployeeName = payslip.PayrollEmployee.Employee.FullName,
            EmployeeCode = payslip.PayrollEmployee.Employee.EmployeeCode,
            DepartmentName = payslip.PayrollEmployee.Employee.ProfessionalDetails?.Department?.Name ?? string.Empty,
            DesignationName = payslip.PayrollEmployee.Employee.ProfessionalDetails?.Designation?.Name ?? string.Empty,
            PayslipNumber = payslip.PayslipNumber,
            Status = payslip.Status,
            GeneratedAt = payslip.GeneratedAt,
            PeriodName = payslip.PayrollEmployee.Period?.Description ?? string.Empty,
            Month = payslip.PayrollEmployee.Period?.Month ?? 0,
            Year = payslip.PayrollEmployee.Period?.Year ?? 0,
            GrossEarnings = payslip.PayrollEmployee.GrossEarnings,
            TotalDeductions = payslip.PayrollEmployee.TotalDeductions,
            NetPay = payslip.PayrollEmployee.NetPay,
            Earnings = payslip.Items
                .Where(i => i.ComponentType == ComponentType.Earning)
                .OrderBy(i => i.DisplayOrder)
                .Select(MapPresentationDto)
                .ToList(),
            Deductions = payslip.Items
                .Where(i => i.ComponentType == ComponentType.Deduction)
                .OrderBy(i => i.DisplayOrder)
                .Select(MapPresentationDto)
                .ToList(),
            EmployerContributions = payslip.Items
                .Where(i => i.ComponentType == ComponentType.EmployerContribution)
                .OrderBy(i => i.DisplayOrder)
                .Select(MapPresentationDto)
                .ToList()
        };

        return Ok(ApiResponseDto<PayslipDto>.Ok(dto));
    }

    /// <summary>
    /// Module 8: Stream binary PDF payslip (logs access to audit table).
    /// </summary>
    [HttpGet("api/payroll/payslips/{id:int}/pdf")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin},{ApplicationRoles.Employee}")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadPayslipPdf(int id, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var currentEmpId = User.GetEmployeeId();

        var payslip = await _context.Payslips
            .Include(p => p.PayrollEmployee)
                .ThenInclude(pe => pe.Period)
            .FirstOrDefaultAsync(p => p.Id == id && p.PayrollEmployee.OrganizationId == orgId, cancellationToken);

        if (payslip == null)
            return NotFound(ApiResponseDto<string>.Fail("Payslip not found."));

        if (!User.IsManagerOrAdmin())
        {
            if (payslip.EmployeeId != currentEmpId)
                return Forbid();

            if (payslip.Status != PayslipStatus.Published)
                return NotFound(ApiResponseDto<string>.Fail("Payslip is not yet published."));
        }

        var pdfBytes = await _pdfGenerator.GeneratePayslipPdfAsync(id, cancellationToken);

        // Record audit download log
        _context.PayslipAccessLogs.Add(new PayslipAccessLog
        {
            PayslipId = payslip.Id,
            AccessedBy = currentEmpId,
            AccessedAt = DateTimeOffset.UtcNow,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            UserAgent = Request.Headers.UserAgent.ToString(),
            ActionType = PayslipActionType.DownloadPdf,
            CreatedBy = currentEmpId
        });
        await _context.SaveChangesAsync(cancellationToken);

        var filename = $"{payslip.PayslipNumber}.pdf";
        return File(pdfBytes, "application/pdf", filename);
    }

    /// <summary>
    /// Module 8: Audit log of who viewed/downloaded this payslip.
    /// </summary>
    [HttpGet("api/payroll/payslips/{id:int}/access-log")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<List<PayslipAccessLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayslipAccessLogs(int id, CancellationToken cancellationToken)
    {
        var logs = await _context.PayslipAccessLogs
            .Where(l => l.PayslipId == id)
            .OrderByDescending(l => l.AccessedAt)
            .ToListAsync(cancellationToken);

        var empIds = logs.Select(l => l.AccessedBy).Distinct().ToList();
        var emps = await _context.Employees
            .Where(e => empIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e.FullName, cancellationToken);

        var dtos = logs.Select(l => new PayslipAccessLogDto
        {
            Id = l.Id,
            PayslipId = l.PayslipId,
            AccessedBy = l.AccessedBy,
            AccessedByName = emps.TryGetValue(l.AccessedBy, out var name) ? name : "System / External",
            AccessedAt = l.AccessedAt,
            IpAddress = l.IpAddress,
            UserAgent = l.UserAgent,
            ActionType = l.ActionType
        }).ToList();

        return Ok(ApiResponseDto<List<PayslipAccessLogDto>>.Ok(dtos));
    }

    /// <summary>
    /// Module 8: Publish payslip for employee self-service viewing.
    /// </summary>
    [HttpPost("api/payroll/payslips/{id:int}/publish")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishPayslip(int id, CancellationToken cancellationToken)
    {
        var payslip = await _context.Payslips.FindAsync(new object[] { id }, cancellationToken);
        if (payslip == null)
            return NotFound(ApiResponseDto<bool>.Fail("Payslip not found."));

        payslip.Status = PayslipStatus.Published;
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseDto<bool>.Ok(true, "Payslip published successfully."));
    }

    /// <summary>
    /// Module 8: Employee Self-Service: list historical payslips for the authenticated employee.
    /// </summary>
    [HttpGet("api/payroll/payslips/my-payslips")]
    [HttpGet("api/payroll/my-payslips")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponseDto<List<MyPayslipSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPayslips([FromQuery] int? year, CancellationToken cancellationToken)
    {
        var currentEmpId = User.TryGetEmployeeId() ?? User.GetEmployeeId();

        var query = _context.Payslips
            .Include(p => p.PayrollEmployee)
                .ThenInclude(pe => pe.Period)
            .Where(p => p.EmployeeId == currentEmpId);

        if (year.HasValue && year.Value > 0)
        {
            query = query.Where(p => p.PayrollEmployee.Period.Year == year.Value);
        }

        // Non-admin employees only see Published payslips
        if (!User.IsManagerOrAdmin())
        {
            query = query.Where(p => p.Status == PayslipStatus.Published);
        }

        var payslips = await query
            .OrderByDescending(p => p.PayrollEmployee.Period.Year)
            .ThenByDescending(p => p.PayrollEmployee.Period.Month)
            .ToListAsync(cancellationToken);

        var dtos = payslips.Select(p => new MyPayslipSummaryDto
        {
            Id = p.Id,
            PayslipNumber = p.PayslipNumber,
            Month = p.PayrollEmployee.Period.Month,
            Year = p.PayrollEmployee.Period.Year,
            PeriodDescription = p.PayrollEmployee.Period.Description,
            GrossEarnings = p.PayrollEmployee.GrossEarnings,
            TotalDeductions = p.PayrollEmployee.TotalDeductions,
            NetPay = p.PayrollEmployee.NetPay,
            Status = p.Status,
            GeneratedAt = p.GeneratedAt
        }).ToList();

        return Ok(ApiResponseDto<List<MyPayslipSummaryDto>>.Ok(dtos));
    }

    /// <summary>
    /// Module 8: Generate draft projection payslips for all calculated employees in a period.
    /// </summary>
    [HttpPost("api/payroll/payslips/period/{periodId:int}/generate")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GeneratePeriodPayslips(int periodId, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var period = await _context.PayrollPeriods
            .FirstOrDefaultAsync(p => p.Id == periodId && p.OrganizationId == orgId, cancellationToken);

        if (period == null)
            return NotFound(ApiResponseDto<int>.Fail("Payroll period not found."));

        var payslips = await _projectionService.ProjectPayslipsForPeriodAsync(periodId, cancellationToken);
        return Ok(ApiResponseDto<int>.Ok(payslips.Count, $"{payslips.Count} payslips generated successfully."));
    }

    /// <summary>
    /// Module 8: Publish all generated payslips for a period so employees can view them.
    /// </summary>
    [HttpPost("api/payroll/payslips/period/{periodId:int}/publish")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishPeriodPayslips(int periodId, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var payslips = await _context.Payslips
            .Include(p => p.PayrollEmployee)
            .Where(p => p.PayrollEmployee.PayrollPeriodId == periodId && p.PayrollEmployee.OrganizationId == orgId)
            .ToListAsync(cancellationToken);

        if (!payslips.Any())
        {
            // If payslips haven't been projected yet, project them first
            payslips = (await _projectionService.ProjectPayslipsForPeriodAsync(periodId, cancellationToken)).ToList();
        }

        foreach (var p in payslips)
        {
            p.Status = PayslipStatus.Published;
        }
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseDto<int>.Ok(payslips.Count, $"{payslips.Count} payslips published successfully."));
    }

    /// <summary>
    /// Module 8: Delete an individual salary slip.
    /// </summary>
    [HttpDelete("api/payroll/payslips/{id:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePayslip(int id, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var payslip = await _context.Payslips
            .Include(p => p.PayrollEmployee)
            .FirstOrDefaultAsync(p => p.Id == id && p.PayrollEmployee.OrganizationId == orgId, cancellationToken);

        if (payslip == null)
            return NotFound(ApiResponseDto<bool>.Fail("Payslip not found."));

        await _context.PayslipItems
            .Where(i => i.PayslipId == id)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.PayslipAccessLogs
            .Where(l => l.PayslipId == id)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.Payslips
            .Where(p => p.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        return Ok(ApiResponseDto<bool>.Ok(true, "Salary slip deleted successfully."));
    }

    /// <summary>
    /// Module 8: Delete all generated payslips for a payroll period.
    /// </summary>
    [HttpDelete("api/payroll/payslips/period/{periodId:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeletePeriodPayslips(int periodId, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var payslipIds = await _context.Payslips
            .Where(p => p.PayrollEmployee.PayrollPeriodId == periodId && p.PayrollEmployee.OrganizationId == orgId)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        if (payslipIds.Any())
        {
            await _context.PayslipItems
                .Where(i => payslipIds.Contains(i.PayslipId))
                .ExecuteDeleteAsync(cancellationToken);

            await _context.PayslipAccessLogs
                .Where(l => payslipIds.Contains(l.PayslipId))
                .ExecuteDeleteAsync(cancellationToken);

            var deletedCount = await _context.Payslips
                .Where(p => payslipIds.Contains(p.Id))
                .ExecuteDeleteAsync(cancellationToken);

            return Ok(ApiResponseDto<int>.Ok(deletedCount, $"{deletedCount} salary slips deleted successfully."));
        }

        return Ok(ApiResponseDto<int>.Ok(0, "No salary slips found for this period."));
    }

    private static PayslipItemPresentationDto MapPresentationDto(PayslipItem i) => new()
    {
        Id = i.Id,
        ComponentCode = i.ComponentCode,
        ComponentName = i.ComponentName,
        ComponentType = i.ComponentType,
        Amount = i.Amount,
        DisplayOrder = i.DisplayOrder
    };
}
