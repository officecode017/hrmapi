using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRAttendance.API.Extensions;
using HRAttendance.Business.BusinessRules;
using HRAttendance.Business.Interfaces;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Employee;

namespace HRAttendance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<PagedResponseDto<EmployeeListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployees([FromQuery] int organizationId = 1, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var request = new PagedRequestDto { PageNumber = pageNumber, PageSize = pageSize };
        var result = await _employeeService.GetEmployeesAsync(organizationId, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponseDto<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _employeeService.GetEmployeeByIdAsync(id, cancellationToken);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<EmployeeDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponseDto<EmployeeDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto request, CancellationToken cancellationToken)
    {
        var result = await _employeeService.CreateEmployeeAsync(request, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto request, CancellationToken cancellationToken)
    {
        var currentEmpId = User.GetEmployeeId();
        var isPrivileged = User.IsInRole(ApplicationRoles.SuperAdmin) 
                        || User.IsInRole(ApplicationRoles.Admin) 
                        || User.IsInRole(ApplicationRoles.Manager)
                        || User.HasClaim("Permission", "*")
                        || User.HasClaim("Permission", "Employee.Manage")
                        || User.HasClaim("Permission", "Employee.Update")
                        || User.HasClaim("Permission", "Employee.Create");

        if (!isPrivileged && currentEmpId != id)
        {
            return Forbid();
        }

        var result = await _employeeService.UpdateEmployeeAsync(id, request, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _employeeService.DeleteEmployeeAsync(id, cancellationToken);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpPost("{id:int}/photo")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponseDto<EmployeePhotoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UploadPhoto(int id, IFormFile file, CancellationToken cancellationToken)
    {
        var currentEmpId = User.GetEmployeeId();
        var isPrivileged = User.IsInRole(ApplicationRoles.SuperAdmin)
                        || User.IsInRole(ApplicationRoles.Admin)
                        || User.IsInRole(ApplicationRoles.Manager)
                        || User.HasClaim("Permission", "*")
                        || User.HasClaim("Permission", "Employee.Manage")
                        || User.HasClaim("Permission", "Employee.Update")
                        || User.HasClaim("Permission", "Employee.EditSelf");

        if (!isPrivileged && currentEmpId != id)
        {
            return Forbid();
        }

        try
        {
            var result = await _employeeService.UpdatePhotoAsync(id, file, cancellationToken);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseDto<EmployeePhotoResponseDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:int}/photo")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemovePhoto(int id, CancellationToken cancellationToken)
    {
        var currentEmpId = User.GetEmployeeId();
        var isPrivileged = User.IsInRole(ApplicationRoles.SuperAdmin)
                        || User.IsInRole(ApplicationRoles.Admin)
                        || User.IsInRole(ApplicationRoles.Manager)
                        || User.HasClaim("Permission", "*")
                        || User.HasClaim("Permission", "Employee.Manage")
                        || User.HasClaim("Permission", "Employee.Update")
                        || User.HasClaim("Permission", "Employee.EditSelf");

        if (!isPrivileged && currentEmpId != id)
        {
            return Forbid();
        }

        var result = await _employeeService.RemovePhotoAsync(id, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{id:int}/background")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponseDto<EmployeeBackgroundResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UploadBackground(int id, IFormFile file, CancellationToken cancellationToken)
    {
        var currentEmpId = User.GetEmployeeId();
        var isPrivileged = User.IsInRole(ApplicationRoles.SuperAdmin)
                        || User.IsInRole(ApplicationRoles.Admin)
                        || User.IsInRole(ApplicationRoles.Manager)
                        || User.HasClaim("Permission", "*")
                        || User.HasClaim("Permission", "Employee.Manage")
                        || User.HasClaim("Permission", "Employee.Update")
                        || User.HasClaim("Permission", "Employee.EditSelf");

        if (!isPrivileged && currentEmpId != id)
        {
            return Forbid();
        }

        try
        {
            var result = await _employeeService.UpdateBackgroundAsync(id, file, cancellationToken);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseDto<EmployeeBackgroundResponseDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:int}/background")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveBackground(int id, CancellationToken cancellationToken)
    {
        var currentEmpId = User.GetEmployeeId();
        var isPrivileged = User.IsInRole(ApplicationRoles.SuperAdmin)
                        || User.IsInRole(ApplicationRoles.Admin)
                        || User.IsInRole(ApplicationRoles.Manager)
                        || User.HasClaim("Permission", "*")
                        || User.HasClaim("Permission", "Employee.Manage")
                        || User.HasClaim("Permission", "Employee.Update")
                        || User.HasClaim("Permission", "Employee.EditSelf");

        if (!isPrivileged && currentEmpId != id)
        {
            return Forbid();
        }

        var result = await _employeeService.RemoveBackgroundAsync(id, cancellationToken);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
