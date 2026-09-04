using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRAttendance.Business.BusinessRules;
using HRAttendance.Business.Interfaces;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.OffDay;

namespace HRAttendance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OffDayController : ControllerBase
{
    private readonly IOffDayService _offDayService;

    public OffDayController(IOffDayService offDayService)
    {
        _offDayService = offDayService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<List<OffDayDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOffDays([FromQuery] int organizationId = 1, [FromQuery] int academicYearId = 1, CancellationToken cancellationToken = default)
    {
        var result = await _offDayService.GetOffDaysAsync(organizationId, academicYearId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponseDto<OffDayDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOffDayById(int id, CancellationToken cancellationToken)
    {
        var result = await _offDayService.GetOffDayByIdAsync(id, cancellationToken);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<OffDayDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOffDay([FromBody] CreateOffDayDto request, CancellationToken cancellationToken)
    {
        var result = await _offDayService.CreateOffDayAsync(request, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetOffDayById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOffDay(int id, [FromBody] UpdateOffDayDto request, CancellationToken cancellationToken)
    {
        var result = await _offDayService.UpdateOffDayAsync(id, request, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteOffDay(int id, CancellationToken cancellationToken)
    {
        var result = await _offDayService.DeleteOffDayAsync(id, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
