using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRAttendance.API.Extensions;
using HRAttendance.Business.BusinessRules;
using HRAttendance.Data;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Payroll;
using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.API.Controllers.Payroll;

[ApiController]
[Authorize]
public class StatutoryRulesAndFiscalYearsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StatutoryRulesAndFiscalYearsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ==========================================
    // STATUTORY RULES
    // ==========================================

    /// <summary>
    /// Module 12: List versioned statutory rules (PF, ESI, PT, TDS).
    /// </summary>
    [HttpGet("api/payroll/statutory-rules")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<List<StatutoryRuleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatutoryRules([FromQuery] StatutoryRuleType? ruleType = null, CancellationToken cancellationToken = default)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var query = _context.StatutoryRules.Where(r => r.OrganizationId == orgId);
        if (ruleType.HasValue)
        {
            query = query.Where(r => r.RuleType == ruleType.Value);
        }

        var rules = await query
            .OrderBy(r => r.RuleType)
            .ThenByDescending(r => r.Version)
            .Select(r => new StatutoryRuleDto
            {
                Id = r.Id,
                OrganizationId = r.OrganizationId,
                RuleType = r.RuleType,
                Version = r.Version,
                EffectiveFrom = r.EffectiveFrom,
                EffectiveTo = r.EffectiveTo,
                ConfigurationJson = r.ConfigurationJson,
                IsActive = r.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponseDto<List<StatutoryRuleDto>>.Ok(rules));
    }

    /// <summary>
    /// Module 12: Create a new statutory rule version with effective date.
    /// </summary>
    [HttpPost("api/payroll/statutory-rules")]
    [Authorize(Roles = ApplicationRoles.SuperAdmin)]
    [ProducesResponseType(typeof(ApiResponseDto<StatutoryRuleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateStatutoryRule([FromBody] CreateStatutoryRuleDto request, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var empId = User.TryGetEmployeeId() ?? 0;

        var latestVersion = await _context.StatutoryRules
            .Where(r => r.OrganizationId == orgId && r.RuleType == request.RuleType)
            .MaxAsync(r => (int?)r.Version, cancellationToken) ?? 0;

        var rule = new StatutoryRule
        {
            OrganizationId = orgId,
            RuleType = request.RuleType,
            Version = latestVersion + 1,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            ConfigurationJson = request.ConfigurationJson,
            IsActive = true,
            CreatedBy = empId
        };

        _context.StatutoryRules.Add(rule);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new StatutoryRuleDto
        {
            Id = rule.Id,
            OrganizationId = rule.OrganizationId,
            RuleType = rule.RuleType,
            Version = rule.Version,
            EffectiveFrom = rule.EffectiveFrom,
            EffectiveTo = rule.EffectiveTo,
            ConfigurationJson = rule.ConfigurationJson,
            IsActive = rule.IsActive
        };

        return CreatedAtAction(nameof(GetStatutoryRuleById), new { id = rule.Id }, ApiResponseDto<StatutoryRuleDto>.Ok(dto, "Statutory rule version created successfully."));
    }

    /// <summary>
    /// Module 12: Get statutory rule configuration JSON.
    /// </summary>
    [HttpGet("api/payroll/statutory-rules/{id:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<StatutoryRuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatutoryRuleById(int id, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var rule = await _context.StatutoryRules
            .FirstOrDefaultAsync(r => r.Id == id && r.OrganizationId == orgId, cancellationToken);

        if (rule == null)
            return NotFound(ApiResponseDto<StatutoryRuleDto>.Fail("Statutory rule not found."));

        var dto = new StatutoryRuleDto
        {
            Id = rule.Id,
            OrganizationId = rule.OrganizationId,
            RuleType = rule.RuleType,
            Version = rule.Version,
            EffectiveFrom = rule.EffectiveFrom,
            EffectiveTo = rule.EffectiveTo,
            ConfigurationJson = rule.ConfigurationJson,
            IsActive = rule.IsActive
        };

        return Ok(ApiResponseDto<StatutoryRuleDto>.Ok(dto));
    }

    /// <summary>
    /// Module 12: Set statutory rule version as active.
    /// </summary>
    [HttpPost("api/payroll/statutory-rules/{id:int}/activate")]
    [Authorize(Roles = ApplicationRoles.SuperAdmin)]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateStatutoryRule(int id, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var rule = await _context.StatutoryRules
            .FirstOrDefaultAsync(r => r.Id == id && r.OrganizationId == orgId, cancellationToken);

        if (rule == null)
            return NotFound(ApiResponseDto<bool>.Fail("Statutory rule not found."));

        rule.IsActive = true;
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseDto<bool>.Ok(true, "Statutory rule activated."));
    }

    /// <summary>
    /// Module 12: Deactivate statutory rule version.
    /// </summary>
    [HttpPost("api/payroll/statutory-rules/{id:int}/deactivate")]
    [Authorize(Roles = ApplicationRoles.SuperAdmin)]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateStatutoryRule(int id, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var rule = await _context.StatutoryRules
            .FirstOrDefaultAsync(r => r.Id == id && r.OrganizationId == orgId, cancellationToken);

        if (rule == null)
            return NotFound(ApiResponseDto<bool>.Fail("Statutory rule not found."));

        rule.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseDto<bool>.Ok(true, "Statutory rule deactivated."));
    }

    // ==========================================
    // FINANCIAL YEARS
    // ==========================================

    /// <summary>
    /// Module 12: List fiscal years (e.g. 2025-26, 2026-27).
    /// </summary>
    [HttpGet("api/payroll/financial-years")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<List<FinancialYearDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFinancialYears(CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var years = await _context.FinancialYears
            .Where(f => f.OrganizationId == orgId)
            .OrderByDescending(f => f.StartDate)
            .Select(f => new FinancialYearDto
            {
                Id = f.Id,
                OrganizationId = f.OrganizationId,
                YearCode = f.YearCode,
                StartDate = f.StartDate,
                EndDate = f.EndDate,
                IsActive = f.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponseDto<List<FinancialYearDto>>.Ok(years));
    }

    /// <summary>
    /// Module 12: Create new fiscal year boundary.
    /// </summary>
    [HttpPost("api/payroll/financial-years")]
    [Authorize(Roles = ApplicationRoles.SuperAdmin)]
    [ProducesResponseType(typeof(ApiResponseDto<FinancialYearDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFinancialYear([FromBody] CreateFinancialYearDto request, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var empId = User.TryGetEmployeeId() ?? 0;

        var exists = await _context.FinancialYears
            .AnyAsync(f => f.OrganizationId == orgId && f.YearCode == request.YearCode, cancellationToken);

        if (exists)
            return BadRequest(ApiResponseDto<FinancialYearDto>.Fail($"Financial year '{request.YearCode}' already exists."));

        if (request.IsActive)
        {
            // Deactivate existing active years
            var activeYears = await _context.FinancialYears
                .Where(f => f.OrganizationId == orgId && f.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var y in activeYears)
                y.IsActive = false;
        }

        var fy = new FinancialYear
        {
            OrganizationId = orgId,
            YearCode = request.YearCode.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            CreatedBy = empId
        };

        _context.FinancialYears.Add(fy);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new FinancialYearDto
        {
            Id = fy.Id,
            OrganizationId = fy.OrganizationId,
            YearCode = fy.YearCode,
            StartDate = fy.StartDate,
            EndDate = fy.EndDate,
            IsActive = fy.IsActive
        };

        return CreatedAtAction(nameof(GetFinancialYearById), new { id = fy.Id }, ApiResponseDto<FinancialYearDto>.Ok(dto, "Financial year created successfully."));
    }

    /// <summary>
    /// Module 12: Get fiscal year details and associated periods.
    /// </summary>
    [HttpGet("api/payroll/financial-years/{id:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<FinancialYearDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFinancialYearById(int id, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var fy = await _context.FinancialYears
            .Include(f => f.PayrollPeriods)
            .FirstOrDefaultAsync(f => f.Id == id && f.OrganizationId == orgId, cancellationToken);

        if (fy == null)
            return NotFound(ApiResponseDto<FinancialYearDto>.Fail("Financial year not found."));

        var dto = new FinancialYearDto
        {
            Id = fy.Id,
            OrganizationId = fy.OrganizationId,
            YearCode = fy.YearCode,
            StartDate = fy.StartDate,
            EndDate = fy.EndDate,
            IsActive = fy.IsActive,
            Periods = fy.PayrollPeriods.Select(p => new PayrollPeriodDto
            {
                Id = p.Id,
                OrganizationId = p.OrganizationId,
                FinancialYearId = p.FinancialYearId,
                YearCode = fy.YearCode,
                Month = p.Month,
                Year = p.Year,
                RunType = p.RunType,
                SequenceNumber = p.SequenceNumber,
                Description = p.Description,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
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
            }).ToList()
        };

        return Ok(ApiResponseDto<FinancialYearDto>.Ok(dto));
    }

    /// <summary>
    /// Module 12: Mark financial year as active current year.
    /// </summary>
    [HttpPost("api/payroll/financial-years/{id:int}/activate")]
    [Authorize(Roles = ApplicationRoles.SuperAdmin)]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateFinancialYear(int id, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var allYears = await _context.FinancialYears
            .Where(f => f.OrganizationId == orgId)
            .ToListAsync(cancellationToken);

        var target = allYears.FirstOrDefault(f => f.Id == id);
        if (target == null)
            return NotFound(ApiResponseDto<bool>.Fail("Financial year not found."));

        foreach (var y in allYears)
        {
            y.IsActive = (y.Id == id);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponseDto<bool>.Ok(true, $"Financial year '{target.YearCode}' activated."));
    }
}
