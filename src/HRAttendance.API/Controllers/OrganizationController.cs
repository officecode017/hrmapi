using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRAttendance.Business.Interfaces;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Organization;
using HRAttendance.Data.DTOs.Department;
using HRAttendance.Data.DTOs.Designation;
using HRAttendance.Data.DTOs.Location;

namespace HRAttendance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrganizationController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public OrganizationController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponseDto<OrganizationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _organizationService.GetOrganizationByIdAsync(id, cancellationToken);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpGet("{id:int}/departments")]
    [ProducesResponseType(typeof(ApiResponseDto<List<DepartmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartments(int id, CancellationToken cancellationToken)
    {
        var result = await _organizationService.GetDepartmentsAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}/designations")]
    [ProducesResponseType(typeof(ApiResponseDto<List<DesignationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDesignations(int id, CancellationToken cancellationToken)
    {
        var result = await _organizationService.GetDesignationsAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}/locations")]
    [ProducesResponseType(typeof(ApiResponseDto<List<LocationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLocations(int id, CancellationToken cancellationToken)
    {
        var result = await _organizationService.GetLocationsAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOrganizationDto request, CancellationToken cancellationToken)
    {
        var result = await _organizationService.UpdateOrganizationAsync(id, request, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
