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
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IPasswordHasher passwordHasher,
        IFileStorageService fileStorageService,
        ILogger<EmployeeService> logger)
    {
        _employeeRepository = employeeRepository;
        _passwordHasher = passwordHasher;
        _fileStorageService = fileStorageService;
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
            PhotoPath = e.PhotoPath,
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
            FatherName = employee.FatherName,
            MotherName = employee.MotherName,
            Nationality = employee.Nationality,
            Religion = employee.Religion,
            BirthPlace = employee.BirthPlace,
            IdentificationMark = employee.IdentificationMark,
            EmployeeType = employee.EmployeeType,
            Qualification = employee.Qualification,
            SkillSet = employee.SkillSet,
            PhotoPath = employee.PhotoPath,
            BackgroundImagePath = employee.BackgroundImagePath,
            IsActive = employee.IsActive,
            ResignationDate = employee.ResignationDate,
            LastWorkingDay = employee.LastWorkingDay,
            ReasonForLeaving = employee.ReasonForLeaving,

            // Contact
            Address = employee.ContactDetails?.Address,
            PermanentAddress = employee.ContactDetails?.PermanentAddress,
            City = employee.ContactDetails?.City,
            State = employee.ContactDetails?.State,
            Country = employee.ContactDetails?.Country,
            PostalCode = employee.ContactDetails?.PostalCode,
            WorkEmail = employee.ContactDetails?.WorkEmail,
            OtherEmail = employee.ContactDetails?.OtherEmail,
            Mobile = employee.ContactDetails?.Mobile,
            WorkTelephone = employee.ContactDetails?.WorkTelephone,
            HomeTelephone = employee.ContactDetails?.HomeTelephone,
            Extension = employee.ContactDetails?.Extension,
            EmergencyPerson = employee.ContactDetails?.EmergencyPerson,
            EmergencyContact = employee.ContactDetails?.EmergencyContact,

            // Professional
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
            ProbationPeriodMonths = employee.ProfessionalDetails?.ProbationPeriod,
            Roles = employee.EmployeeRoles.Select(r => r.Role?.Name ?? string.Empty).Where(r => !string.IsNullOrEmpty(r)).ToList(),
            RoleIds = employee.EmployeeRoles.Select(r => r.RoleId).ToList()
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
            BloodGroup = request.BloodGroup,
            MaritalStatus = request.MaritalStatus,
            FatherName = request.FatherName?.Trim(),
            MotherName = request.MotherName?.Trim(),
            Nationality = request.Nationality?.Trim(),
            Religion = request.Religion?.Trim(),
            BirthPlace = request.BirthPlace?.Trim(),
            IdentificationMark = request.IdentificationMark?.Trim(),
            EmployeeType = request.EmployeeType?.Trim() ?? "Full-Time",
            Qualification = request.Qualification?.Trim(),
            SkillSet = request.SkillSet?.Trim(),
            PhotoPath = request.PhotoPath,
            BackgroundImagePath = request.BackgroundImagePath,
            IsActive = true,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            ContactDetails = new EmployeeContactDetails
            {
                OrganizationId = request.OrganizationId,
                Address = request.Address?.Trim(),
                PermanentAddress = request.PermanentAddress?.Trim(),
                City = request.City?.Trim(),
                State = request.State?.Trim(),
                Country = request.Country?.Trim(),
                PostalCode = request.PostalCode?.Trim(),
                WorkEmail = request.WorkEmail.Trim(),
                OtherEmail = request.OtherEmail?.Trim(),
                Mobile = request.Mobile?.Trim(),
                WorkTelephone = request.WorkTelephone?.Trim(),
                HomeTelephone = request.HomeTelephone?.Trim(),
                Extension = request.Extension?.Trim(),
                EmergencyPerson = request.EmergencyPerson?.Trim(),
                EmergencyContact = request.EmergencyContact?.Trim()
            },
            ProfessionalDetails = new EmployeeProfessionalDetails
            {
                OrganizationId = request.OrganizationId,
                DepartmentId = request.DepartmentId,
                DesignationId = request.DesignationId,
                LocationId = request.LocationId,
                ShiftId = request.ShiftId,
                ReportingTo = request.ReportingTo,
                DateOfJoining = request.DateOfJoining ?? DateOnly.FromDateTime(DateTime.UtcNow),
                ProbationPeriod = request.ProbationPeriodMonths
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
        employee.FatherName = request.FatherName?.Trim();
        employee.MotherName = request.MotherName?.Trim();
        employee.Nationality = request.Nationality?.Trim();
        employee.Religion = request.Religion?.Trim();
        employee.BirthPlace = request.BirthPlace?.Trim();
        employee.IdentificationMark = request.IdentificationMark?.Trim();
        employee.EmployeeType = request.EmployeeType?.Trim();
        employee.Qualification = request.Qualification?.Trim();
        employee.SkillSet = request.SkillSet?.Trim();
        employee.IsActive = request.IsActive;
        employee.ResignationDate = request.ResignationDate;
        employee.LastWorkingDay = request.LastWorkingDay;
        employee.ReasonForLeaving = request.ReasonForLeaving?.Trim();

        if (employee.ContactDetails == null)
        {
            employee.ContactDetails = new EmployeeContactDetails
            {
                OrganizationId = employee.OrganizationId,
                EmployeeId = employee.Id
            };
        }
        employee.ContactDetails.Address = request.Address?.Trim();
        employee.ContactDetails.PermanentAddress = request.PermanentAddress?.Trim();
        employee.ContactDetails.City = request.City?.Trim();
        employee.ContactDetails.State = request.State?.Trim();
        employee.ContactDetails.Country = request.Country?.Trim();
        employee.ContactDetails.PostalCode = request.PostalCode?.Trim();
        employee.ContactDetails.WorkEmail = request.WorkEmail?.Trim();
        employee.ContactDetails.OtherEmail = request.OtherEmail?.Trim();
        employee.ContactDetails.Mobile = request.Mobile?.Trim();
        employee.ContactDetails.WorkTelephone = request.WorkTelephone?.Trim();
        employee.ContactDetails.HomeTelephone = request.HomeTelephone?.Trim();
        employee.ContactDetails.Extension = request.Extension?.Trim();
        employee.ContactDetails.EmergencyPerson = request.EmergencyPerson?.Trim();
        employee.ContactDetails.EmergencyContact = request.EmergencyContact?.Trim();

        if (employee.ProfessionalDetails == null)
        {
            employee.ProfessionalDetails = new EmployeeProfessionalDetails
            {
                OrganizationId = employee.OrganizationId,
                EmployeeId = employee.Id
            };
        }
        employee.ProfessionalDetails.DepartmentId = request.DepartmentId;
        employee.ProfessionalDetails.DesignationId = request.DesignationId;
        employee.ProfessionalDetails.LocationId = request.LocationId;
        employee.ProfessionalDetails.ShiftId = request.ShiftId;
        employee.ProfessionalDetails.ReportingTo = request.ReportingTo;
        employee.ProfessionalDetails.DateOfJoining = request.DateOfJoining;
        employee.ProfessionalDetails.ProbationPeriod = request.ProbationPeriodMonths;

        // Reconcile roles if provided
        if (request.RoleIds != null && request.RoleIds.Any())
        {
            var currentRoleIds = employee.EmployeeRoles.Select(er => er.RoleId).ToList();
            var rolesToRemove = employee.EmployeeRoles.Where(er => !request.RoleIds.Contains(er.RoleId)).ToList();
            foreach (var r in rolesToRemove)
            {
                employee.EmployeeRoles.Remove(r);
            }

            var rolesToAdd = request.RoleIds.Where(rid => !currentRoleIds.Contains(rid)).ToList();
            foreach (var rid in rolesToAdd)
            {
                employee.EmployeeRoles.Add(new EmployeeRole
                {
                    OrganizationId = employee.OrganizationId,
                    EmployeeId = employee.Id,
                    RoleId = rid
                });
            }
        }

        // Reset password if provided
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            employee.PasswordHash = _passwordHasher.HashPassword(request.Password);
        }

        if (request.PhotoPath != null)
        {
            employee.PhotoPath = request.PhotoPath;
        }

        if (request.BackgroundImagePath != null)
        {
            employee.BackgroundImagePath = request.BackgroundImagePath;
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

    public async Task<ApiResponseDto<EmployeePhotoResponseDto>> UpdatePhotoAsync(int id, Microsoft.AspNetCore.Http.IFormFile file, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);
        if (employee == null)
        {
            return ApiResponseDto<EmployeePhotoResponseDto>.Fail($"Employee with ID {id} was not found.");
        }

        var savedPath = await _fileStorageService.SaveFileAsync(file, "profiles", cancellationToken);

        if (!string.IsNullOrWhiteSpace(employee.PhotoPath) && employee.PhotoPath.StartsWith("/uploads/"))
        {
            _fileStorageService.DeleteFile(employee.PhotoPath);
        }

        employee.PhotoPath = savedPath;
        await _employeeRepository.UpdateAsync(employee, cancellationToken);
        _logger.LogInformation("Updated photo for employee {EmployeeId}", id);

        return ApiResponseDto<EmployeePhotoResponseDto>.Ok(new EmployeePhotoResponseDto
        {
            PhotoPath = savedPath
        }, "Profile photo updated successfully.");
    }

    public async Task<ApiResponseDto<bool>> RemovePhotoAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);
        if (employee == null)
        {
            return ApiResponseDto<bool>.Fail($"Employee with ID {id} was not found.");
        }

        if (!string.IsNullOrWhiteSpace(employee.PhotoPath) && employee.PhotoPath.StartsWith("/uploads/"))
        {
            _fileStorageService.DeleteFile(employee.PhotoPath);
        }

        employee.PhotoPath = null;
        await _employeeRepository.UpdateAsync(employee, cancellationToken);
        _logger.LogInformation("Removed photo for employee {EmployeeId}", id);

        return ApiResponseDto<bool>.Ok(true, "Profile photo removed successfully.");
    }

    public async Task<ApiResponseDto<EmployeeBackgroundResponseDto>> UpdateBackgroundAsync(int id, Microsoft.AspNetCore.Http.IFormFile file, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);
        if (employee == null)
        {
            return ApiResponseDto<EmployeeBackgroundResponseDto>.Fail($"Employee with ID {id} was not found.");
        }

        var savedPath = await _fileStorageService.SaveFileAsync(file, "backgrounds", cancellationToken);

        if (!string.IsNullOrWhiteSpace(employee.BackgroundImagePath) && employee.BackgroundImagePath.StartsWith("/uploads/"))
        {
            _fileStorageService.DeleteFile(employee.BackgroundImagePath);
        }

        employee.BackgroundImagePath = savedPath;
        await _employeeRepository.UpdateAsync(employee, cancellationToken);
        _logger.LogInformation("Updated background image for employee {EmployeeId}", id);

        return ApiResponseDto<EmployeeBackgroundResponseDto>.Ok(new EmployeeBackgroundResponseDto
        {
            BackgroundImagePath = savedPath
        }, "Background image updated successfully.");
    }

    public async Task<ApiResponseDto<bool>> RemoveBackgroundAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);
        if (employee == null)
        {
            return ApiResponseDto<bool>.Fail($"Employee with ID {id} was not found.");
        }

        if (!string.IsNullOrWhiteSpace(employee.BackgroundImagePath) && employee.BackgroundImagePath.StartsWith("/uploads/"))
        {
            _fileStorageService.DeleteFile(employee.BackgroundImagePath);
        }

        employee.BackgroundImagePath = null;
        await _employeeRepository.UpdateAsync(employee, cancellationToken);
        _logger.LogInformation("Removed background image for employee {EmployeeId}", id);

        return ApiResponseDto<bool>.Ok(true, "Background image removed successfully.");
    }
}
