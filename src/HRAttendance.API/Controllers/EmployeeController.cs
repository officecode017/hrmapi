using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin},{ApplicationRoles.Manager}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto request, CancellationToken cancellationToken)
    {
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
}
