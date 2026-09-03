using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRAttendance.Business.BusinessRules;
using HRAttendance.Business.Interfaces;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Shift;
using HRAttendance.Data.DTOs.Holiday;
using HRAttendance.Data.DTOs.Role;
using HRAttendance.Data.DTOs.Notification;
using HRAttendance.Data.DTOs.Overtime;

namespace HRAttendance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShiftController : ControllerBase
{
    private readonly IShiftService _shiftService;

    public ShiftController(IShiftService shiftService)
    {
        _shiftService = shiftService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<List<ShiftDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShifts([FromQuery] int organizationId = 1, CancellationToken cancellationToken = default)
    {
        var result = await _shiftService.GetShiftsAsync(organizationId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<ShiftDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateShift([FromBody] CreateShiftDto request, CancellationToken cancellationToken)
    {
        var result = await _shiftService.CreateShiftAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetShifts), new { organizationId = request.OrganizationId }, result);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HolidayController : ControllerBase
{
    private readonly IHolidayService _holidayService;

    public HolidayController(IHolidayService holidayService)
    {
        _holidayService = holidayService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<List<HolidayDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHolidays([FromQuery] int organizationId, [FromQuery] int academicYearId, CancellationToken cancellationToken)
    {
        var result = await _holidayService.GetHolidaysAsync(organizationId, academicYearId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<HolidayDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateHoliday([FromBody] CreateHolidayDto request, CancellationToken cancellationToken)
    {
        var result = await _holidayService.CreateHolidayAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetHolidays), new { organizationId = request.OrganizationId, academicYearId = request.AcademicYearId }, result);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<List<RoleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles([FromQuery] int organizationId = 1, CancellationToken cancellationToken = default)
    {
        var result = await _roleService.GetRolesAsync(organizationId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("assign")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto request, CancellationToken cancellationToken)
    {
        var result = await _roleService.AssignRoleAsync(request, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("{employeeId:int}")]
    [ProducesResponseType(typeof(ApiResponseDto<List<NotificationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications(int employeeId, CancellationToken cancellationToken)
    {
        var result = await _notificationService.GetNotificationsAsync(employeeId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:int}/read")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken cancellationToken)
    {
        var result = await _notificationService.MarkAsReadAsync(id, cancellationToken);
        return Ok(result);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OvertimeController : ControllerBase
{
    private readonly IOvertimeService _overtimeService;

    public OvertimeController(IOvertimeService overtimeService)
    {
        _overtimeService = overtimeService;
    }

    [HttpGet("entries/{employeeId:int}")]
    [ProducesResponseType(typeof(ApiResponseDto<List<OTEntryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEntries(int employeeId, CancellationToken cancellationToken)
    {
        var result = await _overtimeService.GetEntriesAsync(employeeId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("calculate")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin},{ApplicationRoles.Manager}")]
    [ProducesResponseType(typeof(ApiResponseDto<OTEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CalculateOvertime([FromBody] CalculateOvertimeRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _overtimeService.CalculateOvertimeAsync(request, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
