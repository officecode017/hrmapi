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
public class SalaryStructuresController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SalaryStructuresController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Module 4: Assign initial salary structure to an employee.
    /// </summary>
    [HttpPost("api/payroll/salary-structures")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<SalaryStructureDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSalaryStructure([FromBody] CreateSalaryStructureDto request, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var empId = User.TryGetEmployeeId() ?? 0;

        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId && e.OrganizationId == orgId, cancellationToken);
        if (employee == null)
            return BadRequest(ApiResponseDto<SalaryStructureDto>.Fail("Specified employee not found."));

        // Check highest existing version
        var latestVersion = await _context.EmployeeSalaryStructures
            .Where(s => s.EmployeeId == request.EmployeeId && s.OrganizationId == orgId)
            .MaxAsync(s => (int?)s.Version, cancellationToken) ?? 0;

        var newVersion = latestVersion + 1;

        // If there is a previous active structure, set its EffectiveTo if not already set
        var previousActive = await _context.EmployeeSalaryStructures
            .Where(s => s.EmployeeId == request.EmployeeId && s.OrganizationId == orgId && s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (previousActive != null)
        {
            if (previousActive.EffectiveTo == null || previousActive.EffectiveTo >= request.EffectiveFrom)
            {
                previousActive.EffectiveTo = request.EffectiveFrom.AddDays(-1);
            }
            previousActive.IsActive = false;
        }

        var structure = new EmployeeSalaryStructure
        {
            OrganizationId = orgId,
            EmployeeId = request.EmployeeId,
            Version = newVersion,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            MonthlyGrossSalary = request.MonthlyGrossSalary,
            AnnualCTC = request.AnnualCTC,
            RevisionReason = request.RevisionReason ?? (newVersion == 1 ? "Initial Salary Assignment" : "Salary Revision"),
            IsActive = true,
            CreatedBy = empId,
            Items = request.Items.Select(i => new EmployeeSalaryStructureItem
            {
                SalaryComponentId = i.SalaryComponentId,
                MonthlyAmount = i.MonthlyAmount,
                AnnualAmount = i.AnnualAmount,
                PercentageRate = i.PercentageRate,
                CreatedBy = empId
            }).ToList()
        };

        _context.EmployeeSalaryStructures.Add(structure);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetSalaryStructureById(structure.Id, cancellationToken);
    }

    /// <summary>
    /// Module 4: Get specific salary structure details and items.
    /// </summary>
    [HttpGet("api/payroll/salary-structures/{id:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<SalaryStructureDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSalaryStructureById(int id, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var structure = await _context.EmployeeSalaryStructures
            .Include(s => s.Employee)
            .Include(s => s.Items)
                .ThenInclude(i => i.Component)
            .FirstOrDefaultAsync(s => s.Id == id && s.OrganizationId == orgId, cancellationToken);

        if (structure == null)
            return NotFound(ApiResponseDto<SalaryStructureDto>.Fail("Salary structure not found."));

        var dto = new SalaryStructureDto
        {
            Id = structure.Id,
            OrganizationId = structure.OrganizationId,
            EmployeeId = structure.EmployeeId,
            EmployeeName = structure.Employee.FullName,
            EmployeeCode = structure.Employee.EmployeeCode,
            Version = structure.Version,
            EffectiveFrom = structure.EffectiveFrom,
            EffectiveTo = structure.EffectiveTo,
            MonthlyGrossSalary = structure.MonthlyGrossSalary,
            AnnualCTC = structure.AnnualCTC,
            RevisionReason = structure.RevisionReason,
            IsActive = structure.IsActive,
            Items = structure.Items.Select(i => new SalaryStructureItemDto
            {
                Id = i.Id,
                SalaryComponentId = i.SalaryComponentId,
                ComponentCode = i.Component.Code,
                ComponentName = i.Component.Name,
                ComponentType = i.Component.Type,
                MonthlyAmount = i.MonthlyAmount,
                AnnualAmount = i.AnnualAmount,
                PercentageRate = i.PercentageRate
            }).ToList()
        };

        return Ok(ApiResponseDto<SalaryStructureDto>.Ok(dto));
    }

    /// <summary>
    /// Module 4: List all active salary structures in organization.
    /// </summary>
    [HttpGet("api/payroll/salary-structures")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<List<SalaryStructureDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllActiveSalaryStructures(CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var structures = await _context.EmployeeSalaryStructures
            .Include(s => s.Employee)
            .Include(s => s.Items)
                .ThenInclude(i => i.Component)
            .Where(s => s.OrganizationId == orgId && s.IsActive)
            .OrderBy(s => s.Employee.FirstName)
            .ToListAsync(cancellationToken);

        var dtos = structures.Select(structure => new SalaryStructureDto
        {
            Id = structure.Id,
            OrganizationId = structure.OrganizationId,
            EmployeeId = structure.EmployeeId,
            EmployeeName = structure.Employee.FullName,
            EmployeeCode = structure.Employee.EmployeeCode,
            Version = structure.Version,
            EffectiveFrom = structure.EffectiveFrom,
            EffectiveTo = structure.EffectiveTo,
            MonthlyGrossSalary = structure.MonthlyGrossSalary,
            AnnualCTC = structure.AnnualCTC,
            RevisionReason = structure.RevisionReason,
            IsActive = structure.IsActive,
            Items = structure.Items.Select(i => new SalaryStructureItemDto
            {
                Id = i.Id,
                SalaryComponentId = i.SalaryComponentId,
                ComponentCode = i.Component.Code,
                ComponentName = i.Component.Name,
                ComponentType = i.Component.Type,
                MonthlyAmount = i.MonthlyAmount,
                AnnualAmount = i.AnnualAmount,
                PercentageRate = i.PercentageRate
            }).ToList()
        }).ToList();

        return Ok(ApiResponseDto<List<SalaryStructureDto>>.Ok(dtos));
    }

    /// <summary>
    /// Module 4: Create a new version of salary structure with effective date (Revision).
    /// </summary>
    [HttpPost("api/payroll/salary-structures/{id:int}/revise")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<SalaryStructureDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReviseSalaryStructure(int id, [FromBody] ReviseSalaryStructureDto request, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var existing = await _context.EmployeeSalaryStructures
            .FirstOrDefaultAsync(s => s.Id == id && s.OrganizationId == orgId, cancellationToken);

        if (existing == null)
            return NotFound(ApiResponseDto<SalaryStructureDto>.Fail("Base salary structure not found."));

        var createDto = new CreateSalaryStructureDto
        {
            EmployeeId = existing.EmployeeId,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            MonthlyGrossSalary = request.MonthlyGrossSalary,
            AnnualCTC = request.AnnualCTC,
            RevisionReason = request.RevisionReason,
            Items = request.Items
        };

        return await CreateSalaryStructure(createDto, cancellationToken);
    }

    /// <summary>
    /// Module 4: List all historical versions for an employee.
    /// </summary>
    [HttpGet("api/payroll/employees/{employeeId:int}/salary-structures")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<List<SalaryStructureDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeSalaryStructures(int employeeId, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var structures = await _context.EmployeeSalaryStructures
            .Include(s => s.Employee)
            .Include(s => s.Items)
                .ThenInclude(i => i.Component)
            .Where(s => s.EmployeeId == employeeId && s.OrganizationId == orgId)
            .OrderByDescending(s => s.Version)
            .ToListAsync(cancellationToken);

        var dtos = structures.Select(structure => new SalaryStructureDto
        {
            Id = structure.Id,
            OrganizationId = structure.OrganizationId,
            EmployeeId = structure.EmployeeId,
            EmployeeName = structure.Employee.FullName,
            EmployeeCode = structure.Employee.EmployeeCode,
            Version = structure.Version,
            EffectiveFrom = structure.EffectiveFrom,
            EffectiveTo = structure.EffectiveTo,
            MonthlyGrossSalary = structure.MonthlyGrossSalary,
            AnnualCTC = structure.AnnualCTC,
            RevisionReason = structure.RevisionReason,
            IsActive = structure.IsActive,
            Items = structure.Items.Select(i => new SalaryStructureItemDto
            {
                Id = i.Id,
                SalaryComponentId = i.SalaryComponentId,
                ComponentCode = i.Component.Code,
                ComponentName = i.Component.Name,
                ComponentType = i.Component.Type,
                MonthlyAmount = i.MonthlyAmount,
                AnnualAmount = i.AnnualAmount,
                PercentageRate = i.PercentageRate
            }).ToList()
        }).ToList();

        return Ok(ApiResponseDto<List<SalaryStructureDto>>.Ok(dtos));
    }

    /// <summary>
    /// Module 4: Get current active salary structure for an employee.
    /// </summary>
    [HttpGet("api/payroll/employees/{employeeId:int}/salary-structures/current")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin},{ApplicationRoles.Employee}")]
    [ProducesResponseType(typeof(ApiResponseDto<SalaryStructureDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentActiveStructure(int employeeId, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var currentEmpId = User.GetEmployeeId();

        // Enforce employee authorization
        if (!User.IsManagerOrAdmin() && currentEmpId != employeeId)
            return Forbid();

        var structure = await _context.EmployeeSalaryStructures
            .Include(s => s.Employee)
            .Include(s => s.Items)
                .ThenInclude(i => i.Component)
            .Where(s => s.EmployeeId == employeeId && s.OrganizationId == orgId && s.IsActive)
            .OrderByDescending(s => s.Version)
            .FirstOrDefaultAsync(cancellationToken);

        if (structure == null)
            return NotFound(ApiResponseDto<SalaryStructureDto>.Fail("No active salary structure found for employee."));

        var dto = new SalaryStructureDto
        {
            Id = structure.Id,
            OrganizationId = structure.OrganizationId,
            EmployeeId = structure.EmployeeId,
            EmployeeName = structure.Employee.FullName,
            EmployeeCode = structure.Employee.EmployeeCode,
            Version = structure.Version,
            EffectiveFrom = structure.EffectiveFrom,
            EffectiveTo = structure.EffectiveTo,
            MonthlyGrossSalary = structure.MonthlyGrossSalary,
            AnnualCTC = structure.AnnualCTC,
            RevisionReason = structure.RevisionReason,
            IsActive = structure.IsActive,
            Items = structure.Items.Select(i => new SalaryStructureItemDto
            {
                Id = i.Id,
                SalaryComponentId = i.SalaryComponentId,
                ComponentCode = i.Component.Code,
                ComponentName = i.Component.Name,
                ComponentType = i.Component.Type,
                MonthlyAmount = i.MonthlyAmount,
                AnnualAmount = i.AnnualAmount,
                PercentageRate = i.PercentageRate
            }).ToList()
        };

        return Ok(ApiResponseDto<SalaryStructureDto>.Ok(dto));
    }

    /// <summary>
    /// Module 4: Timeline of revisions, salary hikes, and CTC growth.
    /// </summary>
    [HttpGet("api/payroll/employees/{employeeId:int}/salary-history")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<SalaryHistoryTimelineDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSalaryHistoryTimeline(int employeeId, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId && e.OrganizationId == orgId, cancellationToken);
        if (employee == null)
            return NotFound(ApiResponseDto<SalaryHistoryTimelineDto>.Fail("Employee not found."));

        var structures = await _context.EmployeeSalaryStructures
            .Where(s => s.EmployeeId == employeeId && s.OrganizationId == orgId)
            .OrderBy(s => s.Version)
            .ToListAsync(cancellationToken);

        var historyItems = new List<SalaryHistoryItemDto>();
        decimal previousGross = 0;

        foreach (var s in structures)
        {
            decimal hike = 0;
            if (previousGross > 0)
            {
                hike = Math.Round(((s.MonthlyGrossSalary - previousGross) / previousGross) * 100m, 2);
            }

            historyItems.Add(new SalaryHistoryItemDto
            {
                StructureId = s.Id,
                Version = s.Version,
                EffectiveFrom = s.EffectiveFrom,
                EffectiveTo = s.EffectiveTo,
                MonthlyGrossSalary = s.MonthlyGrossSalary,
                AnnualCTC = s.AnnualCTC,
                RevisionReason = s.RevisionReason,
                PercentageHike = hike,
                IsActive = s.IsActive
            });

            previousGross = s.MonthlyGrossSalary;
        }

        var timeline = new SalaryHistoryTimelineDto
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.FullName,
            EmployeeCode = employee.EmployeeCode,
            History = historyItems.OrderByDescending(h => h.Version).ToList()
        };

        return Ok(ApiResponseDto<SalaryHistoryTimelineDto>.Ok(timeline));
    }
}
