using Microsoft.Extensions.Logging;
using HRAttendance.Business.Interfaces;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Employee;
using HRAttendance.Data.Interfaces;
using HRAttendance.Data.Models.Employee;
using HRAttendance.Data.Models.Security;

namespace HRAttendance.Business.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IPasswordHasher passwordHasher,
        ILogger<EmployeeService> logger)
    {
        _employeeRepository = employeeRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<ApiResponseDto<PagedResponseDto<EmployeeListDto>>> GetEmployeesAsync(int organizationId, PagedRequestDto request, CancellationToken cancellationToken = default)
    {
        var skip = (request.PageNumber - 1) * request.PageSize;
        var totalCount = await _employeeRepository.CountAsync(organizationId, cancellationToken);
        var employees = await _employeeRepository.GetAllAsync(organizationId, skip, request.PageSize, cancellationToken);

        var dtos = employees.Select(e => new EmployeeListDto
        {
            Id = e.Id,
            OrganizationId = e.OrganizationId,
            EmployeeCode = e.EmployeeCode,
            FullName = $"{e.FirstName} {e.LastName}".Trim(),
            WorkEmail = e.ContactDetails?.WorkEmail,
            Mobile = e.ContactDetails?.Mobile,
            DepartmentName = e.ProfessionalDetails?.Department?.Name,
            DesignationName = e.ProfessionalDetails?.Designation?.Name,
            IsActive = e.IsActive
        }).ToList();

        var pagedResponse = new PagedResponseDto<EmployeeListDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return ApiResponseDto<PagedResponseDto<EmployeeListDto>>.Ok(pagedResponse);
    }

    public async Task<ApiResponseDto<EmployeeDto>> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetWithDetailsAsync(id, cancellationToken);
        if (employee == null)
        {
            return ApiResponseDto<EmployeeDto>.Fail($"Employee with ID {id} was not found.");
        }

        var dto = new EmployeeDto
        {
            Id = employee.Id,
            OrganizationId = employee.OrganizationId,
            EmployeeCode = employee.EmployeeCode,
            FirstName = employee.FirstName,
            MiddleName = employee.MiddleName,
            LastName = employee.LastName,
            DOB = employee.DOB,
            Gender = employee.Gender,
            BloodGroup = employee.BloodGroup,
            MaritalStatus = employee.MaritalStatus,
            IsActive = employee.IsActive,
            WorkEmail = employee.ContactDetails?.WorkEmail,
            Mobile = employee.ContactDetails?.Mobile,
            City = employee.ContactDetails?.City,
            Country = employee.ContactDetails?.Country,
            DepartmentId = employee.ProfessionalDetails?.DepartmentId,
            DepartmentName = employee.ProfessionalDetails?.Department?.Name,
            DesignationId = employee.ProfessionalDetails?.DesignationId,
            DesignationName = employee.ProfessionalDetails?.Designation?.Name,
            LocationId = employee.ProfessionalDetails?.LocationId,
            LocationName = employee.ProfessionalDetails?.Location?.Name,
            ShiftId = employee.ProfessionalDetails?.ShiftId,
            ShiftName = employee.ProfessionalDetails?.Shift?.Name,
            ReportingTo = employee.ProfessionalDetails?.ReportingTo,
            ReportingToName = employee.ProfessionalDetails?.Manager != null ? $"{employee.ProfessionalDetails.Manager.FirstName} {employee.ProfessionalDetails.Manager.LastName}".Trim() : null,
            DateOfJoining = employee.ProfessionalDetails?.DateOfJoining,
            Roles = employee.EmployeeRoles.Select(r => r.Role?.Name ?? string.Empty).Where(r => !string.IsNullOrEmpty(r)).ToList()
        };

        return ApiResponseDto<EmployeeDto>.Ok(dto);
    }

    public async Task<ApiResponseDto<EmployeeDto>> CreateEmployeeAsync(CreateEmployeeDto request, CancellationToken cancellationToken = default)
    {
        var codeExists = await _employeeRepository.ExistsAsync(request.OrganizationId, request.EmployeeCode, null, cancellationToken);
        if (codeExists)
        {
            return ApiResponseDto<EmployeeDto>.Fail($"Employee code '{request.EmployeeCode}' is already registered.");
        }

        var employee = new Employee
        {
            OrganizationId = request.OrganizationId,
            EmployeeCode = request.EmployeeCode.Trim(),
            FirstName = request.FirstName.Trim(),
            MiddleName = request.MiddleName?.Trim(),
            LastName = request.LastName.Trim(),
            DOB = request.DOB,
            Gender = request.Gender,
            IsActive = true,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            ContactDetails = new EmployeeContactDetails
            {
                OrganizationId = request.OrganizationId,
                WorkEmail = request.WorkEmail.Trim(),
                Mobile = request.Mobile?.Trim(),
                City = request.City?.Trim(),
                Country = request.Country?.Trim()
            },
            ProfessionalDetails = new EmployeeProfessionalDetails
            {
                OrganizationId = request.OrganizationId,
                DepartmentId = request.DepartmentId,
                DesignationId = request.DesignationId,
                LocationId = request.LocationId,
                ShiftId = request.ShiftId,
                ReportingTo = request.ReportingTo,
                DateOfJoining = request.DateOfJoining ?? DateOnly.FromDateTime(DateTime.UtcNow)
            }
        };

        if (request.RoleIds != null && request.RoleIds.Any())
        {
            foreach (var roleId in request.RoleIds)
            {
                employee.EmployeeRoles.Add(new EmployeeRole
                {
                    OrganizationId = request.OrganizationId,
                    RoleId = roleId
                });
            }
        }

        await _employeeRepository.AddAsync(employee, cancellationToken);
        _logger.LogInformation("Created new employee {EmployeeId} ({Code})", employee.Id, employee.EmployeeCode);

        return await GetEmployeeByIdAsync(employee.Id, cancellationToken);
    }

    public async Task<ApiResponseDto<bool>> UpdateEmployeeAsync(int id, UpdateEmployeeDto request, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetWithDetailsAsync(id, cancellationToken);
        if (employee == null)
        {
            return ApiResponseDto<bool>.Fail($"Employee with ID {id} was not found.");
        }

        employee.FirstName = request.FirstName.Trim();
        employee.MiddleName = request.MiddleName?.Trim();
        employee.LastName = request.LastName.Trim();
        employee.DOB = request.DOB;
        employee.Gender = request.Gender;
        employee.BloodGroup = request.BloodGroup;
        employee.MaritalStatus = request.MaritalStatus;
        employee.IsActive = request.IsActive;

        if (employee.ContactDetails != null)
        {
            employee.ContactDetails.WorkEmail = request.WorkEmail?.Trim();
            employee.ContactDetails.Mobile = request.Mobile?.Trim();
            employee.ContactDetails.City = request.City?.Trim();
            employee.ContactDetails.Country = request.Country?.Trim();
        }

        if (employee.ProfessionalDetails != null)
        {
            employee.ProfessionalDetails.DepartmentId = request.DepartmentId;
            employee.ProfessionalDetails.DesignationId = request.DesignationId;
            employee.ProfessionalDetails.LocationId = request.LocationId;
            employee.ProfessionalDetails.ShiftId = request.ShiftId;
            employee.ProfessionalDetails.ReportingTo = request.ReportingTo;
        }

        await _employeeRepository.UpdateAsync(employee, cancellationToken);
        _logger.LogInformation("Updated employee {EmployeeId}", id);

        return ApiResponseDto<bool>.Ok(true, "Employee updated successfully.");
    }

    public async Task<ApiResponseDto<bool>> DeleteEmployeeAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);
        if (employee == null)
        {
            return ApiResponseDto<bool>.Fail($"Employee with ID {id} was not found.");
        }

        await _employeeRepository.DeleteAsync(employee, cancellationToken);
        _logger.LogInformation("Soft-deleted employee {EmployeeId}", id);

        return ApiResponseDto<bool>.Ok(true, "Employee deleted successfully.");
    }
}
