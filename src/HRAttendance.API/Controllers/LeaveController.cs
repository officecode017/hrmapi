using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRAttendance.Business.BusinessRules;
using HRAttendance.Business.Interfaces;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Leave;
using HRAttendance.API.Extensions;

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
    public async Task<IActionResult> GetLeaveBalances([FromQuery] int employeeId, [FromQuery] int? academicYearId, CancellationToken cancellationToken)
    {
        var result = await _leaveService.GetLeaveBalancesAsync(employeeId, academicYearId ?? 0, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelLeave(int id, CancellationToken cancellationToken)
    {
        var employeeId = User.TryGetEmployeeId() ?? 1;
        var isAdmin = User.IsInRole(ApplicationRoles.SuperAdmin) || User.IsInRole(ApplicationRoles.Admin);
        var result = await _leaveService.CancelLeaveAsync(id, employeeId, isAdmin, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("balances/adjust")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AdjustLeaveBalance([FromBody] AdjustLeaveBalanceDto request, CancellationToken cancellationToken)
    {
        var result = await _leaveService.AdjustLeaveBalanceAsync(request, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("balances/all")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin},{ApplicationRoles.Manager}")]
    [ProducesResponseType(typeof(ApiResponseDto<List<EmployeeLeaveBalanceDetailDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllLeaveBalances(
        [FromQuery] int organizationId = 1,
        [FromQuery] int? academicYearId = null,
        [FromQuery] int? departmentId = null,
        [FromQuery] int? leaveTypeId = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _leaveService.GetAllLeaveBalancesAsync(organizationId, academicYearId, departmentId, leaveTypeId, search, cancellationToken);
        return Ok(result);
    }

    [HttpPost("balances/bulk-allocate")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkAllocateLeaveBalances([FromBody] BulkAllocateLeaveBalanceDto request, CancellationToken cancellationToken)
    {
        var result = await _leaveService.BulkAllocateLeaveBalancesAsync(request, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    private int GetCurrentUserId()

    {
        return User.TryGetEmployeeId() ?? 1;
    }
}
