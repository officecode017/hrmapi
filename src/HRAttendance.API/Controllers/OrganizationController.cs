using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRAttendance.Business.BusinessRules;
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

    [HttpGet("public-branding")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponseDto<OrganizationBrandingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublicBranding([FromQuery] int? organizationId, CancellationToken cancellationToken)
    {
        var result = await _organizationService.GetOrganizationBrandingAsync(organizationId, cancellationToken);
        return Ok(result);
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

    [HttpPost("{id:int}/logo")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponseDto<OrganizationLogoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UploadLogo(int id, IFormFile file, CancellationToken cancellationToken)
    {
        var isPrivileged = User.IsInRole(ApplicationRoles.SuperAdmin)
                        || User.IsInRole(ApplicationRoles.Admin)
                        || User.HasClaim("Permission", "*")
                        || User.HasClaim("Permission", "Organization.Update")
                        || User.HasClaim("Permission", "Organization.Manage");

        if (!isPrivileged)
        {
            return Forbid();
        }

        try
        {
            var result = await _organizationService.UpdateLogoAsync(id, file, cancellationToken);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseDto<OrganizationLogoResponseDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:int}/logo")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveLogo(int id, CancellationToken cancellationToken)
    {
        var isPrivileged = User.IsInRole(ApplicationRoles.SuperAdmin)
                        || User.IsInRole(ApplicationRoles.Admin)
                        || User.HasClaim("Permission", "*")
                        || User.HasClaim("Permission", "Organization.Update")
                        || User.HasClaim("Permission", "Organization.Manage");

        if (!isPrivileged)
        {
            return Forbid();
        }

        var result = await _organizationService.RemoveLogoAsync(id, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
