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
[Route("api/payroll/salary-components")]
[Authorize]
public class SalaryComponentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SalaryComponentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Module 3: List all earnings, deductions, and statutory components.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<List<SalaryComponentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComponents([FromQuery] bool? activeOnly = null, CancellationToken cancellationToken = default)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var query = _context.SalaryComponents.Where(c => c.OrganizationId == orgId);
        if (activeOnly.HasValue && activeOnly.Value)
        {
            query = query.Where(c => c.IsActive);
        }

        var items = await query
            .OrderBy(c => c.CalculationOrder)
            .Select(c => new SalaryComponentDto
            {
                Id = c.Id,
                OrganizationId = c.OrganizationId,
                Code = c.Code,
                Name = c.Name,
                Type = c.Type,
                CalculationType = c.CalculationType,
                CalculationOrder = c.CalculationOrder,
                IsTaxable = c.IsTaxable,
                IsStatutory = c.IsStatutory,
                IsActive = c.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponseDto<List<SalaryComponentDto>>.Ok(items));
    }

    /// <summary>
    /// Module 3: Create a custom salary component.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<SalaryComponentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateComponent([FromBody] CreateSalaryComponentDto request, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var empId = User.TryGetEmployeeId() ?? 0;

        var normalizedCode = request.Code.Trim().ToUpperInvariant();

        var exists = await _context.SalaryComponents.AnyAsync(c =>
            c.OrganizationId == orgId && c.Code == normalizedCode, cancellationToken);

        if (exists)
            return BadRequest(ApiResponseDto<SalaryComponentDto>.Fail($"Salary component code '{normalizedCode}' already exists."));

        var component = new SalaryComponent
        {
            OrganizationId = orgId,
            Code = normalizedCode,
            Name = request.Name.Trim(),
            Type = request.Type,
            CalculationType = request.CalculationType,
            CalculationOrder = request.CalculationOrder,
            IsTaxable = request.IsTaxable,
            IsStatutory = request.IsStatutory,
            IsActive = true,
            CreatedBy = empId
        };

        _context.SalaryComponents.Add(component);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new SalaryComponentDto
        {
            Id = component.Id,
            OrganizationId = component.OrganizationId,
            Code = component.Code,
            Name = component.Name,
            Type = component.Type,
            CalculationType = component.CalculationType,
            CalculationOrder = component.CalculationOrder,
            IsTaxable = component.IsTaxable,
            IsStatutory = component.IsStatutory,
            IsActive = component.IsActive
        };

        return CreatedAtAction(nameof(GetComponentById), new { id = component.Id }, ApiResponseDto<SalaryComponentDto>.Ok(dto, "Component created successfully."));
    }

    /// <summary>
    /// Module 3: Get salary component details.
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<SalaryComponentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComponentById(int id, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var component = await _context.SalaryComponents.FirstOrDefaultAsync(c => c.Id == id && c.OrganizationId == orgId, cancellationToken);
        if (component == null)
            return NotFound(ApiResponseDto<SalaryComponentDto>.Fail("Salary component not found."));

        var dto = new SalaryComponentDto
        {
            Id = component.Id,
            OrganizationId = component.OrganizationId,
            Code = component.Code,
            Name = component.Name,
            Type = component.Type,
            CalculationType = component.CalculationType,
            CalculationOrder = component.CalculationOrder,
            IsTaxable = component.IsTaxable,
            IsStatutory = component.IsStatutory,
            IsActive = component.IsActive
        };

        return Ok(ApiResponseDto<SalaryComponentDto>.Ok(dto));
    }

    /// <summary>
    /// Module 3: Update salary component properties (order, taxability, name).
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<SalaryComponentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateComponent(int id, [FromBody] UpdateSalaryComponentDto request, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var empId = User.TryGetEmployeeId() ?? 0;

        var component = await _context.SalaryComponents.FirstOrDefaultAsync(c => c.Id == id && c.OrganizationId == orgId, cancellationToken);
        if (component == null)
            return NotFound(ApiResponseDto<SalaryComponentDto>.Fail("Salary component not found."));

        component.Name = request.Name.Trim();
        component.Type = request.Type;
        component.CalculationType = request.CalculationType;
        component.CalculationOrder = request.CalculationOrder;
        component.IsTaxable = request.IsTaxable;
        component.IsStatutory = request.IsStatutory;
        component.IsActive = request.IsActive;
        component.ModifiedBy = empId;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new SalaryComponentDto
        {
            Id = component.Id,
            OrganizationId = component.OrganizationId,
            Code = component.Code,
            Name = component.Name,
            Type = component.Type,
            CalculationType = component.CalculationType,
            CalculationOrder = component.CalculationOrder,
            IsTaxable = component.IsTaxable,
            IsStatutory = component.IsStatutory,
            IsActive = component.IsActive
        };

        return Ok(ApiResponseDto<SalaryComponentDto>.Ok(dto, "Component updated successfully."));
    }

    /// <summary>
    /// Module 3: Deactivate component from future salary assignments.
    /// </summary>
    [HttpPost("{id:int}/deactivate")]
    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateComponent(int id, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var empId = User.TryGetEmployeeId() ?? 0;

        var component = await _context.SalaryComponents.FirstOrDefaultAsync(c => c.Id == id && c.OrganizationId == orgId, cancellationToken);
        if (component == null)
            return NotFound(ApiResponseDto<bool>.Fail("Salary component not found."));

        component.IsActive = false;
        component.ModifiedBy = empId;

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponseDto<bool>.Ok(true, "Salary component deactivated successfully."));
    }
}
