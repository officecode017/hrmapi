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
[Route("api/payroll/policy")]
[Authorize]
public class PayrollPolicyController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PayrollPolicyController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Module 11: Get organization's active calculation policy.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<PayrollPolicyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPolicy(CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var policy = await _context.PayrollPolicies
            .FirstOrDefaultAsync(p => p.OrganizationId == orgId, cancellationToken);

        if (policy == null)
        {
            // Auto-provision default policy if none exists yet
            policy = new PayrollPolicy
            {
                OrganizationId = orgId,
                ProrationBasis = SalaryProrationBasis.ActualCalendarDays,
                FixedProrationDays = 30,
                LOPBasis = LOPCalculationBasis.CalendarDays,
                FixedLOPDays = 30,
                OTBasis = OvertimeBasis.BasicSalary,
                OTMultiplier = 1.5m,
                StandardMonthlyWorkingHours = 160m,
                RoundingRule = RoundingRule.TwoDecimals,
                ConsiderHolidaysInLOP = false,
                ConsiderWeekendsInLOP = false,
                CreatedBy = User.TryGetEmployeeId() ?? 0
            };
            _context.PayrollPolicies.Add(policy);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var dto = new PayrollPolicyDto
        {
            Id = policy.Id,
            OrganizationId = policy.OrganizationId,
            ProrationBasis = policy.ProrationBasis,
            FixedProrationDays = policy.FixedProrationDays,
            LOPBasis = policy.LOPBasis,
            FixedLOPDays = policy.FixedLOPDays,
            OTBasis = policy.OTBasis,
            OTMultiplier = policy.OTMultiplier,
            StandardMonthlyWorkingHours = policy.StandardMonthlyWorkingHours,
            RoundingRule = policy.RoundingRule,
            ConsiderHolidaysInLOP = policy.ConsiderHolidaysInLOP,
            ConsiderWeekendsInLOP = policy.ConsiderWeekendsInLOP,
            ModifiedAt = policy.ModifiedAt,
            ModifiedBy = policy.ModifiedBy
        };

        return Ok(ApiResponseDto<PayrollPolicyDto>.Ok(dto));
    }

    /// <summary>
    /// Module 11: Create initial organization policy.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = ApplicationRoles.SuperAdmin)]
    [ProducesResponseType(typeof(ApiResponseDto<PayrollPolicyDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePolicy([FromBody] CreateOrUpdatePayrollPolicyDto request, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var empId = User.TryGetEmployeeId() ?? 0;

        var existing = await _context.PayrollPolicies
            .FirstOrDefaultAsync(p => p.OrganizationId == orgId, cancellationToken);

        if (existing != null)
            return BadRequest(ApiResponseDto<PayrollPolicyDto>.Fail("Payroll policy already exists for this organization. Use PUT to update."));

        var policy = new PayrollPolicy
        {
            OrganizationId = orgId,
            ProrationBasis = request.ProrationBasis,
            FixedProrationDays = request.FixedProrationDays,
            LOPBasis = request.LOPBasis,
            FixedLOPDays = request.FixedLOPDays,
            OTBasis = request.OTBasis,
            OTMultiplier = request.OTMultiplier,
            StandardMonthlyWorkingHours = request.StandardMonthlyWorkingHours,
            RoundingRule = request.RoundingRule,
            ConsiderHolidaysInLOP = request.ConsiderHolidaysInLOP,
            ConsiderWeekendsInLOP = request.ConsiderWeekendsInLOP,
            CreatedBy = empId
        };

        _context.PayrollPolicies.Add(policy);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new PayrollPolicyDto
        {
            Id = policy.Id,
            OrganizationId = policy.OrganizationId,
            ProrationBasis = policy.ProrationBasis,
            FixedProrationDays = policy.FixedProrationDays,
            LOPBasis = policy.LOPBasis,
            FixedLOPDays = policy.FixedLOPDays,
            OTBasis = policy.OTBasis,
            OTMultiplier = policy.OTMultiplier,
            StandardMonthlyWorkingHours = policy.StandardMonthlyWorkingHours,
            RoundingRule = policy.RoundingRule,
            ConsiderHolidaysInLOP = policy.ConsiderHolidaysInLOP,
            ConsiderWeekendsInLOP = policy.ConsiderWeekendsInLOP
        };

        return CreatedAtAction(nameof(GetPolicy), ApiResponseDto<PayrollPolicyDto>.Ok(dto, "Policy created successfully."));
    }

    /// <summary>
    /// Module 11: Update proration basis, LOP divisor, OT multiplier.
    /// </summary>
    [HttpPut]
    [Authorize(Roles = ApplicationRoles.SuperAdmin)]
    [ProducesResponseType(typeof(ApiResponseDto<PayrollPolicyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePolicy([FromBody] CreateOrUpdatePayrollPolicyDto request, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var empId = User.TryGetEmployeeId() ?? 0;

        var policy = await _context.PayrollPolicies
            .FirstOrDefaultAsync(p => p.OrganizationId == orgId, cancellationToken);

        if (policy == null)
            return NotFound(ApiResponseDto<PayrollPolicyDto>.Fail("Payroll policy not found. Create one first."));

        policy.ProrationBasis = request.ProrationBasis;
        policy.FixedProrationDays = request.FixedProrationDays;
        policy.LOPBasis = request.LOPBasis;
        policy.FixedLOPDays = request.FixedLOPDays;
        policy.OTBasis = request.OTBasis;
        policy.OTMultiplier = request.OTMultiplier;
        policy.StandardMonthlyWorkingHours = request.StandardMonthlyWorkingHours;
        policy.RoundingRule = request.RoundingRule;
        policy.ConsiderHolidaysInLOP = request.ConsiderHolidaysInLOP;
        policy.ConsiderWeekendsInLOP = request.ConsiderWeekendsInLOP;
        policy.ModifiedBy = empId;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new PayrollPolicyDto
        {
            Id = policy.Id,
            OrganizationId = policy.OrganizationId,
            ProrationBasis = policy.ProrationBasis,
            FixedProrationDays = policy.FixedProrationDays,
            LOPBasis = policy.LOPBasis,
            FixedLOPDays = policy.FixedLOPDays,
            OTBasis = policy.OTBasis,
            OTMultiplier = policy.OTMultiplier,
            StandardMonthlyWorkingHours = policy.StandardMonthlyWorkingHours,
            RoundingRule = policy.RoundingRule,
            ConsiderHolidaysInLOP = policy.ConsiderHolidaysInLOP,
            ConsiderWeekendsInLOP = policy.ConsiderWeekendsInLOP,
            ModifiedAt = policy.ModifiedAt,
            ModifiedBy = policy.ModifiedBy
        };

        return Ok(ApiResponseDto<PayrollPolicyDto>.Ok(dto, "Payroll policy updated successfully."));
    }

    /// <summary>
    /// Module 11: Audit log of policy configuration modifications.
    /// </summary>
    [HttpGet("history")]
    [Authorize(Roles = ApplicationRoles.SuperAdmin)]
    [ProducesResponseType(typeof(ApiResponseDto<List<PayrollPolicyHistoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPolicyHistory(CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var policy = await _context.PayrollPolicies
            .FirstOrDefaultAsync(p => p.OrganizationId == orgId, cancellationToken);

        var history = new List<PayrollPolicyHistoryDto>();
        if (policy != null)
        {
            var user = await _context.Employees
                .Where(e => e.Id == (policy.ModifiedBy ?? policy.CreatedBy))
                .Select(e => (e.FirstName ?? "") + " " + (e.LastName ?? ""))
                .FirstOrDefaultAsync(cancellationToken);

            var dt = policy.ModifiedAt ?? policy.CreatedAt ?? DateTime.UtcNow;

            history.Add(new PayrollPolicyHistoryDto
            {
                Id = policy.Id,
                Timestamp = new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc)),
                ModifiedBy = policy.ModifiedBy ?? policy.CreatedBy,
                ModifiedByName = user ?? "System",
                ChangeSummary = $"Policy active: Proration={policy.ProrationBasis}, LOP={policy.LOPBasis}, OTMultiplier={policy.OTMultiplier}"
            });
        }

        return Ok(ApiResponseDto<List<PayrollPolicyHistoryDto>>.Ok(history));
    }
}
