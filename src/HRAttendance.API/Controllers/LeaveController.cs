using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRAttendance.Business.BusinessRules;
using HRAttendance.Business.Interfaces;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Leave;

namespace HRAttendance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaveController : ControllerBase
{
    private readonly ILeaveService _leaveService;

    public LeaveController(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    [HttpGet("my-leaves/{employeeId:int}")]
    [ProducesResponseType(typeof(ApiResponseDto<List<LeaveApplicationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeLeaves(int employeeId, CancellationToken cancellationToken)
    {
        var result = await _leaveService.GetEmployeeLeavesAsync(employeeId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("pending")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin},{ApplicationRoles.Manager}")]
    [ProducesResponseType(typeof(ApiResponseDto<List<LeaveApplicationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingLeaves([FromQuery] int organizationId = 1, CancellationToken cancellationToken = default)
    {
        var result = await _leaveService.GetPendingLeavesAsync(organizationId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseDto<LeaveApplicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApplyLeave([FromBody] CreateLeaveApplicationDto request, CancellationToken cancellationToken)
    {
        var result = await _leaveService.ApplyLeaveAsync(request, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin},{ApplicationRoles.Manager}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveLeave(int id, [FromBody] LeaveApprovalDto request, CancellationToken cancellationToken)
    {
        request.IsApproved = true;
        var approverId = GetCurrentUserId();
        var result = await _leaveService.ProcessLeaveApprovalAsync(id, approverId, request, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{id:int}/reject")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin},{ApplicationRoles.Manager}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RejectLeave(int id, [FromBody] LeaveApprovalDto request, CancellationToken cancellationToken)
    {
        request.IsApproved = false;
        var approverId = GetCurrentUserId();
        var result = await _leaveService.ProcessLeaveApprovalAsync(id, approverId, request, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("balances")]
    [ProducesResponseType(typeof(ApiResponseDto<List<LeaveBalanceDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaveBalances([FromQuery] int employeeId, [FromQuery] int academicYearId, CancellationToken cancellationToken)
    {
        var result = await _leaveService.GetLeaveBalancesAsync(employeeId, academicYearId, cancellationToken);
        return Ok(result);
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : 1;
    }
}
