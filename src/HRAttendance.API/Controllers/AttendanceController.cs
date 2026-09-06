using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRAttendance.Business.Interfaces;
using HRAttendance.Data.DTOs.Attendance;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.API.Extensions;
using HRAttendance.Business.BusinessRules;

namespace HRAttendance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpPost("check-in")]
    [ProducesResponseType(typeof(ApiResponseDto<AttendanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequestDto request, CancellationToken cancellationToken)
    {
        // Enforce employee from claims unless manager/admin explicitly punches on behalf of an employee
        if (request.EmployeeId <= 0 || !User.IsManagerOrAdmin())
        {
            request.EmployeeId = User.GetEmployeeId();
        }

        var result = await _attendanceService.CheckInAsync(request, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("check-out")]
    [ProducesResponseType(typeof(ApiResponseDto<AttendanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutRequestDto request, CancellationToken cancellationToken)
    {
        if (request.EmployeeId <= 0 || !User.IsManagerOrAdmin())
        {
            request.EmployeeId = User.GetEmployeeId();
        }

        var result = await _attendanceService.CheckOutAsync(request, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponseDto<List<AttendanceHistoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int? employeeId, 
        [FromQuery] DateOnly? fromDate, 
        [FromQuery] DateOnly? toDate, 
        CancellationToken cancellationToken)
    {
        // Default to the authenticated employee's ID from claims.
        // Only managers/admins are allowed to query attendance history for other employees.
        var targetEmployeeId = (!User.IsManagerOrAdmin() || !employeeId.HasValue || employeeId.Value <= 0)
            ? User.GetEmployeeId()
            : employeeId.Value;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var from = fromDate ?? today.AddDays(-30);
        var to = toDate ?? today;

        var result = await _attendanceService.GetHistoryAsync(targetEmployeeId, from, to, cancellationToken);
        return Ok(result);
    }

    [HttpGet("today")]
    [ProducesResponseType(typeof(ApiResponseDto<AttendanceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetToday(CancellationToken cancellationToken)
    {
        var employeeId = User.GetEmployeeId();
        var result = await _attendanceService.GetTodayStatusAsync(employeeId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("today/{employeeId:int}")]
    [Authorize(Roles = "Super Admin,HR/Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponseDto<AttendanceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTodayForEmployee(int employeeId, CancellationToken cancellationToken)
    {
        var result = await _attendanceService.GetTodayStatusAsync(employeeId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("admin/mark")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<AttendanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AdminMarkAttendance([FromBody] AdminMarkAttendanceRequestDto request, CancellationToken cancellationToken)
    {
        if (request.EmployeeId <= 0)
        {
            return BadRequest(ApiResponseDto<AttendanceDto>.Fail("Employee ID is required."));
        }

        var adminUserId = User.GetEmployeeId();
        var result = await _attendanceService.AdminMarkAttendanceAsync(request, adminUserId, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
