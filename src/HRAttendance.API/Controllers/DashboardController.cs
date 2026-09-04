using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRAttendance.API.Extensions;
using HRAttendance.Business.BusinessRules;
using HRAttendance.Business.Interfaces;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Dashboard;

namespace HRAttendance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("employee")]
    [ProducesResponseType(typeof(ApiResponseDto<EmployeeDashboardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEmployeeDashboard(CancellationToken cancellationToken)
    {
        var employeeId = User.TryGetEmployeeId() ?? 1;
        var result = await _dashboardService.GetEmployeeDashboardAsync(employeeId, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("employee/{employeeId:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin},{ApplicationRoles.Manager}")]
    [ProducesResponseType(typeof(ApiResponseDto<EmployeeDashboardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEmployeeDashboardById(int employeeId, CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetEmployeeDashboardAsync(employeeId, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("employee/calendar")]
    [ProducesResponseType(typeof(ApiResponseDto<EmployeeCalendarDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEmployeeCalendar([FromQuery] int? year, [FromQuery] int? month, CancellationToken cancellationToken)
    {
        var employeeId = User.TryGetEmployeeId() ?? 1;
        var y = year ?? DateTime.UtcNow.Year;
        var m = month ?? DateTime.UtcNow.Month;

        var result = await _dashboardService.GetEmployeeCalendarAsync(employeeId, y, m, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("employee/{employeeId:int}/calendar")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin},{ApplicationRoles.Manager}")]
    [ProducesResponseType(typeof(ApiResponseDto<EmployeeCalendarDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEmployeeCalendarById(int employeeId, [FromQuery] int? year, [FromQuery] int? month, CancellationToken cancellationToken)
    {
        var y = year ?? DateTime.UtcNow.Year;
        var m = month ?? DateTime.UtcNow.Month;

        var result = await _dashboardService.GetEmployeeCalendarAsync(employeeId, y, m, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
