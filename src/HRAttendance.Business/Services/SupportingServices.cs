using Microsoft.EntityFrameworkCore;
using HRAttendance.Business.Interfaces;
using HRAttendance.Data;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Shift;
using HRAttendance.Data.DTOs.Holiday;
using HRAttendance.Data.DTOs.Role;
using HRAttendance.Data.DTOs.Notification;
using HRAttendance.Data.DTOs.Overtime;
using HRAttendance.Data.DTOs.Department;
using HRAttendance.Data.DTOs.Designation;
using HRAttendance.Data.DTOs.Location;
using HRAttendance.Data.DTOs.AcademicYear;
using HRAttendance.Data.DTOs.OffDay;
using HRAttendance.Data.DTOs.Leave;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Security;
using HRAttendance.Data.Models.Overtime;
using HRAttendance.Data.Models.Leave;
using HRAttendance.Data.Models.Employee;

namespace HRAttendance.Business.Services;

public class ShiftService : IShiftService
{
    private readonly ApplicationDbContext _context;

    public ShiftService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponseDto<List<ShiftDto>>> GetShiftsAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        var shifts = await _context.Shifts
            .AsNoTracking()
            .Include(s => s.Location)
            .Where(s => s.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        var dtos = shifts.Select(s => new ShiftDto
        {
            Id = s.Id,
            OrganizationId = s.OrganizationId,
            LocationId = s.LocationId,
            LocationName = s.Location?.Name,
            Name = s.Name,
            InTime = s.InTime,
            OutTime = s.OutTime,
            IsOvernight = s.IsOvernight,
            GraceMinutes = s.GraceMinutes,
            BreakMinutes = s.BreakMinutes,
            BreakStartTime = s.BreakStartTime,
            BreakEndTime = s.BreakEndTime
        }).ToList();

        return ApiResponseDto<List<ShiftDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<ShiftDto>> GetShiftByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var s = await _context.Shifts
            .AsNoTracking()
            .Include(s => s.Location)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (s == null)
        {
            s = await _context.Shifts
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        if (s == null) return ApiResponseDto<ShiftDto>.Fail("Shift not found.");

        var dto = new ShiftDto
        {
            Id = s.Id,
            OrganizationId = s.OrganizationId,
            LocationId = s.LocationId,
            LocationName = s.Location?.Name,
            Name = s.Name,
            InTime = s.InTime,
            OutTime = s.OutTime,
            IsOvernight = s.IsOvernight,
            GraceMinutes = s.GraceMinutes,
            BreakMinutes = s.BreakMinutes,
            BreakStartTime = s.BreakStartTime,
            BreakEndTime = s.BreakEndTime
        };

        return ApiResponseDto<ShiftDto>.Ok(dto);
    }

    public async Task<ApiResponseDto<ShiftDto>> CreateShiftAsync(CreateShiftDto request, CancellationToken cancellationToken = default)
    {
        var shift = new Shift
        {
            OrganizationId = request.OrganizationId,
            LocationId = request.LocationId,
            Name = request.Name.Trim(),
            InTime = request.InTime,
            OutTime = request.OutTime,
            IsOvernight = request.IsOvernight,
            GraceMinutes = request.GraceMinutes,
            BreakMinutes = request.BreakMinutes,
            BreakStartTime = request.BreakStartTime,
            BreakEndTime = request.BreakEndTime
        };

        await _context.Shifts.AddAsync(shift, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new ShiftDto
        {
            Id = shift.Id,
            OrganizationId = shift.OrganizationId,
            LocationId = shift.LocationId,
            Name = shift.Name,
            InTime = shift.InTime,
            OutTime = shift.OutTime,
            IsOvernight = shift.IsOvernight,
            GraceMinutes = shift.GraceMinutes,
            BreakMinutes = shift.BreakMinutes,
            BreakStartTime = shift.BreakStartTime,
            BreakEndTime = shift.BreakEndTime
        };

        return ApiResponseDto<ShiftDto>.Ok(dto, "Shift created successfully.");
    }

    public async Task<ApiResponseDto<bool>> UpdateShiftAsync(int id, UpdateShiftDto request, CancellationToken cancellationToken = default)
    {
        var shift = await _context.Shifts.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (shift == null) return ApiResponseDto<bool>.Fail("Shift not found.");

        shift.LocationId = request.LocationId;
        shift.Name = request.Name.Trim();
        shift.InTime = request.InTime;
        shift.OutTime = request.OutTime;
        shift.IsOvernight = request.IsOvernight;
        shift.GraceMinutes = request.GraceMinutes;
        shift.BreakMinutes = request.BreakMinutes;
        shift.BreakStartTime = request.BreakStartTime;
        shift.BreakEndTime = request.BreakEndTime;

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseDto<bool>.Ok(true, "Shift updated successfully.");
    }

    public async Task<ApiResponseDto<bool>> DeleteShiftAsync(int id, CancellationToken cancellationToken = default)
    {
        var shift = await _context.Shifts.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (shift == null) return ApiResponseDto<bool>.Fail("Shift not found.");

        shift.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseDto<bool>.Ok(true, "Shift deleted successfully.");
    }

    public async Task<ApiResponseDto<bool>> AssignShiftAsync(AssignShiftDto request, CancellationToken cancellationToken = default)
    {
        var employee = await _context.Employees
            .Include(e => e.ProfessionalDetails)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null) return ApiResponseDto<bool>.Fail("Employee not found.");

        var shiftExists = await _context.Shifts.AnyAsync(s => s.Id == request.ShiftId, cancellationToken);
        if (!shiftExists) return ApiResponseDto<bool>.Fail("Shift not found.");

        if (employee.ProfessionalDetails == null)
        {
            employee.ProfessionalDetails = new EmployeeProfessionalDetails
            {
                OrganizationId = employee.OrganizationId,
                EmployeeId = employee.Id,
                ShiftId = request.ShiftId
            };
            await _context.EmployeeProfessionalDetails.AddAsync(employee.ProfessionalDetails, cancellationToken);
        }
        else
        {
            employee.ProfessionalDetails.ShiftId = request.ShiftId;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseDto<bool>.Ok(true, "Shift assigned successfully.");
    }

    public async Task<ApiResponseDto<bool>> BulkAssignShiftAsync(BulkAssignShiftDto request, CancellationToken cancellationToken = default)
    {
        var shiftExists = await _context.Shifts.AnyAsync(s => s.Id == request.ShiftId, cancellationToken);
        if (!shiftExists) return ApiResponseDto<bool>.Fail("Shift not found.");

        var employees = await _context.Employees
            .Include(e => e.ProfessionalDetails)
            .Where(e => request.EmployeeIds.Contains(e.Id))
            .ToListAsync(cancellationToken);

        foreach (var employee in employees)
        {
            if (employee.ProfessionalDetails == null)
            {
                employee.ProfessionalDetails = new EmployeeProfessionalDetails
                {
                    OrganizationId = employee.OrganizationId,
                    EmployeeId = employee.Id,
                    ShiftId = request.ShiftId
                };
                await _context.EmployeeProfessionalDetails.AddAsync(employee.ProfessionalDetails, cancellationToken);
            }
            else
            {
                employee.ProfessionalDetails.ShiftId = request.ShiftId;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseDto<bool>.Ok(true, $"Shift assigned to {employees.Count} employees successfully.");
    }
}

public class HolidayService : IHolidayService
{
    private readonly ApplicationDbContext _context;

    public HolidayService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponseDto<List<HolidayDto>>> GetHolidaysAsync(int organizationId, int academicYearId, CancellationToken cancellationToken = default)
    {
        var holidays = await _context.Holidays
            .AsNoTracking()
            .Include(h => h.Location)
            .Where(h => h.OrganizationId == organizationId && h.AcademicYearId == academicYearId)
            .OrderBy(h => h.Date)
            .ToListAsync(cancellationToken);

        var dtos = holidays.Select(h => new HolidayDto
        {
            Id = h.Id,
            OrganizationId = h.OrganizationId,
            AcademicYearId = h.AcademicYearId,
            LocationId = h.LocationId,
            LocationName = h.Location?.Name,
            Name = h.Name,
            Date = h.Date,
            IsOptional = h.IsOptional
        }).ToList();

        return ApiResponseDto<List<HolidayDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<HolidayDto>> GetHolidayByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var h = await _context.Holidays
            .AsNoTracking()
            .Include(h => h.Location)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

        if (h == null) return ApiResponseDto<HolidayDto>.Fail("Holiday not found.");

        var dto = new HolidayDto
        {
            Id = h.Id,
            OrganizationId = h.OrganizationId,
            AcademicYearId = h.AcademicYearId,
            LocationId = h.LocationId,
            LocationName = h.Location?.Name,
            Name = h.Name,
            Date = h.Date,
            IsOptional = h.IsOptional
        };

        return ApiResponseDto<HolidayDto>.Ok(dto);
    }

    public async Task<ApiResponseDto<HolidayDto>> CreateHolidayAsync(CreateHolidayDto request, CancellationToken cancellationToken = default)
    {
        var holiday = new Holiday
        {
            OrganizationId = request.OrganizationId,
            LocationId = request.LocationId,
            AcademicYearId = request.AcademicYearId,
            Name = request.Name.Trim(),
            Date = request.Date,
            IsOptional = request.IsOptional
        };

        await _context.Holidays.AddAsync(holiday, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetHolidayByIdAsync(holiday.Id, cancellationToken);
    }

    public async Task<ApiResponseDto<bool>> UpdateHolidayAsync(int id, UpdateHolidayDto request, CancellationToken cancellationToken = default)
    {
        var holiday = await _context.Holidays.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
        if (holiday == null) return ApiResponseDto<bool>.Fail("Holiday not found.");

        holiday.LocationId = request.LocationId;
        holiday.Name = request.Name.Trim();
        holiday.Date = request.Date;
        holiday.IsOptional = request.IsOptional;

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseDto<bool>.Ok(true, "Holiday updated successfully.");
    }

    public async Task<ApiResponseDto<bool>> DeleteHolidayAsync(int id, CancellationToken cancellationToken = default)
    {
        var holiday = await _context.Holidays.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
        if (holiday == null) return ApiResponseDto<bool>.Fail("Holiday not found.");

        holiday.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseDto<bool>.Ok(true, "Holiday deleted successfully.");
    }
}

public class RoleService : IRoleService
{
    private readonly ApplicationDbContext _context;

    public RoleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponseDto<List<RoleDto>>> GetRolesAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        var roles = await _context.Roles
            .AsNoTracking()
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Where(r => r.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        var dtos = roles.Select(r => new RoleDto
        {
            Id = r.Id,
            OrganizationId = r.OrganizationId,
            Name = r.Name,
            Description = r.Description,
            IsActive = r.IsActive,
            Permissions = r.RolePermissions.Select(rp => new PermissionDto
            {
                Id = rp.PermissionId,
                Module = rp.Permission?.Category ?? "General",
                Action = rp.Permission?.Name ?? string.Empty,
                Description = rp.Permission?.Description
            }).ToList()
        }).ToList();

        return ApiResponseDto<List<RoleDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<RoleDto>> GetRoleByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var r = await _context.Roles
            .AsNoTracking()
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (r == null) return ApiResponseDto<RoleDto>.Fail("Role not found.");

        var dto = new RoleDto
        {
            Id = r.Id,
            OrganizationId = r.OrganizationId,
            Name = r.Name,
            Description = r.Description,
            IsActive = r.IsActive,
            Permissions = r.RolePermissions.Select(rp => new PermissionDto
            {
                Id = rp.PermissionId,
                Module = rp.Permission?.Category ?? "General",
                Action = rp.Permission?.Name ?? string.Empty,
                Description = rp.Permission?.Description
            }).ToList()
        };

        return ApiResponseDto<RoleDto>.Ok(dto);
    }

    public async Task<ApiResponseDto<RoleDto>> CreateRoleAsync(CreateRoleDto request, CancellationToken cancellationToken = default)
    {
        var role = new Role
        {
            OrganizationId = request.OrganizationId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            IsActive = request.IsActive
        };

        if (request.PermissionIds.Any())
        {
            foreach (var permId in request.PermissionIds)
            {
                role.RolePermissions.Add(new RolePermission
                {
                    OrganizationId = request.OrganizationId,
                    PermissionId = permId
                });
            }
        }

        await _context.Roles.AddAsync(role, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetRoleByIdAsync(role.Id, cancellationToken);
    }

    public async Task<ApiResponseDto<bool>> UpdateRoleAsync(int id, UpdateRoleDto request, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (role == null) return ApiResponseDto<bool>.Fail("Role not found.");

        role.Name = request.Name.Trim();
        role.Description = request.Description?.Trim();
        role.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseDto<bool>.Ok(true, "Role updated successfully.");
    }

    public async Task<ApiResponseDto<bool>> DeleteRoleAsync(int id, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (role == null) return ApiResponseDto<bool>.Fail("Role not found.");

        role.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseDto<bool>.Ok(true, "Role deleted successfully.");
    }

    public async Task<ApiResponseDto<bool>> AssignRoleAsync(AssignRoleDto request, CancellationToken cancellationToken = default)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);
        if (employee == null) return ApiResponseDto<bool>.Fail("Employee not found.");

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);
        if (role == null) return ApiResponseDto<bool>.Fail("Role not found.");

        var existing = await _context.EmployeeRoles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(er => er.EmployeeId == request.EmployeeId && er.RoleId == request.RoleId, cancellationToken);

        if (existing == null)
        {
            await _context.EmployeeRoles.AddAsync(new EmployeeRole
            {
                OrganizationId = employee.OrganizationId,
                EmployeeId = request.EmployeeId,
                RoleId = request.RoleId
            }, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }
        else if (existing.IsDeleted)
        {
            existing.IsDeleted = false;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return ApiResponseDto<bool>.Ok(true, "Role assigned successfully.");
    }

    public async Task<ApiResponseDto<List<PermissionDto>>> GetPermissionsAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        var perms = await _context.PermissionMasters
            .AsNoTracking()
            .Where(p => p.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        var dtos = perms.Select(p => new PermissionDto
        {
            Id = p.Id,
            Module = p.Category ?? "General",
            Action = p.Name,
            Description = p.Description
        }).ToList();

        return ApiResponseDto<List<PermissionDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<bool>> AssignRolePermissionsAsync(AssignRolePermissionsDto request, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);
        if (role == null) return ApiResponseDto<bool>.Fail("Role not found.");

        var existing = await _context.RolePermissions
            .IgnoreQueryFilters()
            .Where(rp => rp.RoleId == request.RoleId)
            .ToListAsync(cancellationToken);

        var requestedPermIds = request.PermissionIds.Distinct().ToHashSet();

        // 1. Mark unselected as deleted
        foreach (var rp in existing.Where(rp => !requestedPermIds.Contains(rp.PermissionId) && !rp.IsDeleted))
        {
            rp.IsDeleted = true;
        }

        // 2. Reactivate previously deleted ones
        foreach (var rp in existing.Where(rp => requestedPermIds.Contains(rp.PermissionId) && rp.IsDeleted))
        {
            rp.IsDeleted = false;
        }

        // 3. Add brand new permissions
        var existingPermIds = existing.Select(rp => rp.PermissionId).ToHashSet();
        foreach (var permId in requestedPermIds.Where(p => !existingPermIds.Contains(p)))
        {
            _context.RolePermissions.Add(new RolePermission
            {
                OrganizationId = role.OrganizationId,
                RoleId = role.Id,
                PermissionId = permId,
                IsDeleted = false
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseDto<bool>.Ok(true, "Role permissions updated successfully.");
    }
}

public class OvertimeService : IOvertimeService
{
    private readonly ApplicationDbContext _context;

    public OvertimeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponseDto<OTSettingDto?>> GetSettingsAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        var setting = await _context.OTSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.OrganizationId == organizationId && s.IsActive, cancellationToken);

        if (setting == null) return ApiResponseDto<OTSettingDto?>.Ok(null);

        var dto = new OTSettingDto
        {
            Id = setting.Id,
            OrganizationId = setting.OrganizationId,
            Name = setting.Name,
            IsOverTimeEnabled = setting.IsOverTimeEnabled,
            OTStartAfterMinutes = setting.OTStartAfterMinutes,
            Multiplier = setting.Multiplier,
            MaxOTHoursPerDay = setting.MaxOTHoursPerDay ?? 4.00m,
            IsActive = setting.IsActive
        };

        return ApiResponseDto<OTSettingDto?>.Ok(dto);
    }

    public async Task<ApiResponseDto<OTSettingDto>> CreateOTSettingAsync(CreateOTSettingDto request, CancellationToken cancellationToken = default)
    {
        var setting = new OTSetting
        {
            OrganizationId = request.OrganizationId,
            Name = request.Name.Trim(),
            IsOverTimeEnabled = request.IsOverTimeEnabled,
            OTStartAfterMinutes = request.OTStartAfterMinutes,
            Multiplier = request.Multiplier,
            MaxOTHoursPerDay = request.MaxOTHoursPerDay,
            IsActive = request.IsActive
        };

        await _context.OTSettings.AddAsync(setting, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new OTSettingDto
        {
            Id = setting.Id,
            OrganizationId = setting.OrganizationId,
            Name = setting.Name,
            IsOverTimeEnabled = setting.IsOverTimeEnabled,
            OTStartAfterMinutes = setting.OTStartAfterMinutes,
            Multiplier = setting.Multiplier,
            MaxOTHoursPerDay = setting.MaxOTHoursPerDay ?? 4.00m,
            IsActive = setting.IsActive
        };

        return ApiResponseDto<OTSettingDto>.Ok(dto, "OT Settings created successfully.");
    }

    public async Task<ApiResponseDto<bool>> UpdateOTSettingAsync(int id, UpdateOTSettingDto request, CancellationToken cancellationToken = default)
    {
        var setting = await _context.OTSettings.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (setting == null) return ApiResponseDto<bool>.Fail("OT Setting not found.");

        setting.Name = request.Name.Trim();
        setting.IsOverTimeEnabled = request.IsOverTimeEnabled;
        setting.OTStartAfterMinutes = request.OTStartAfterMinutes;
        setting.Multiplier = request.Multiplier;
        setting.MaxOTHoursPerDay = request.MaxOTHoursPerDay;
        setting.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseDto<bool>.Ok(true, "OT Setting updated successfully.");
    }

    public async Task<ApiResponseDto<bool>> RecordOTEntryAsync(CreateOTEntryDto request, CancellationToken cancellationToken = default)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);
        if (employee == null) return ApiResponseDto<bool>.Fail("Employee not found.");

        var setting = await _context.OTSettings.FirstOrDefaultAsync(s => s.Id == request.OTSettingId, cancellationToken);
        if (setting == null) return ApiResponseDto<bool>.Fail("OT Setting not found.");

        var entry = new OTEntry
        {
            OrganizationId = employee.OrganizationId,
            EmployeeId = request.EmployeeId,
            OTSettingId = request.OTSettingId,
            OTDate = request.OTDate,
            OTHours = request.OTHours,
            MultiplierApplied = setting.Multiplier,
            HourlyRate = request.HourlyRate,
            OTAmount = Math.Round(request.OTHours * request.HourlyRate * setting.Multiplier, 2)
        };

        await _context.OTEntries.AddAsync(entry, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponseDto<bool>.Ok(true, "OT Entry recorded successfully.");
    }

    public async Task<ApiResponseDto<List<OTEntryDto>>> GetPendingOTEntriesAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        var entries = await _context.OTEntries
            .AsNoTracking()
            .Include(o => o.Employee)
            .Include(o => o.OTSetting)
            .Where(o => o.OrganizationId == organizationId)
            .OrderByDescending(o => o.OTDate)
            .ToListAsync(cancellationToken);

        var dtos = entries.Select(o => new OTEntryDto
        {
            Id = o.Id,
            OrganizationId = o.OrganizationId,
            EmployeeId = o.EmployeeId,
            EmployeeName = $"{o.Employee.FirstName} {o.Employee.LastName}".Trim(),
            OTSettingId = o.OTSettingId,
            OTSettingName = o.OTSetting?.Name,
            OTDate = o.OTDate,
            OTHours = o.OTHours,
            MultiplierApplied = o.MultiplierApplied,
            HourlyRate = o.HourlyRate,
            OTAmount = o.OTAmount,
            Status = o.ModifiedBy.HasValue ? "Approved" : "Pending"
        }).ToList();

        return ApiResponseDto<List<OTEntryDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<bool>> ProcessOTApprovalAsync(int id, int approverId, ApproveOTEntryDto request, CancellationToken cancellationToken = default)
    {
        var entry = await _context.OTEntries.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (entry == null) return ApiResponseDto<bool>.Fail("OT Entry not found.");

        if (request.IsApproved)
        {
            entry.ModifiedBy = approverId;
            entry.ModifiedAt = DateTime.UtcNow;
        }
        else
        {
            entry.IsDeleted = true; // Rejected entry removed from calculations
        }

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseDto<bool>.Ok(true, request.IsApproved ? "OT Entry approved successfully." : "OT Entry rejected.");
    }
}

public class DepartmentService : IDepartmentService
{
    private readonly ApplicationDbContext _context;

    public DepartmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponseDto<List<DepartmentDto>>> GetDepartmentsAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        var departments = await _context.Departments
            .AsNoTracking()
            .Include(d => d.DepartmentHead)
            .Where(d => d.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        var dtos = departments.Select(d => new DepartmentDto
        {
            Id = d.Id,
            OrganizationId = d.OrganizationId,
            Name = d.Name,
            DepartmentHeadId = d.DepartmentHeadId,
            DepartmentHeadName = d.DepartmentHead != null ? $"{d.DepartmentHead.FirstName} {d.DepartmentHead.LastName}".Trim() : null
        }).ToList();

        return ApiResponseDto<List<DepartmentDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<DepartmentDto>> GetDepartmentByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var d = await _context.Departments
            .AsNoTracking()
            .Include(d => d.DepartmentHead)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (d == null) return ApiResponseDto<DepartmentDto>.Fail("Department not found.");

        var dto = new DepartmentDto
        {
            Id = d.Id,
            OrganizationId = d.OrganizationId,
            Name = d.Name,
            DepartmentHeadId = d.DepartmentHeadId,
            DepartmentHeadName = d.DepartmentHead != null ? $"{d.DepartmentHead.FirstName} {d.DepartmentHead.LastName}".Trim() : null
        };

        return ApiResponseDto<DepartmentDto>.Ok(dto);
    }

    public async Task<ApiResponseDto<DepartmentDto>> CreateDepartmentAsync(CreateDepartmentDto request, CancellationToken cancellationToken = default)
    {
        var department = new Department
        {
            OrganizationId = request.OrganizationId,
            Name = request.Name.Trim(),
            DepartmentHeadId = request.DepartmentHeadId
        };

        await _context.Departments.AddAsync(department, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetDepartmentByIdAsync(department.Id, cancellationToken);
    }

    public async Task<ApiResponseDto<bool>> UpdateDepartmentAsync(int id, UpdateDepartmentDto request, CancellationToken cancellationToken = default)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (department == null) return ApiResponseDto<bool>.Fail("Department not found.");

        department.Name = request.Name.Trim();
        department.DepartmentHeadId = request.DepartmentHeadId;

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseDto<bool>.Ok(true, "Department updated successfully.");
    }

    public async Task<ApiResponseDto<bool>> DeleteDepartmentAsync(int id, CancellationToken cancellationToken = default)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (department == null) return ApiResponseDto<bool>.Fail("Department not found.");

        department.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseDto<bool>.Ok(true, "Department deleted successfully.");
    }
}

public class DesignationService : IDesignationService
{
    private readonly ApplicationDbContext _context;

    public DesignationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponseDto<List<DesignationDto>>> GetDesignationsAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        var designations = await _context.Designations
            .AsNoTracking()
            .Where(d => d.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        var dtos = designations.Select(d => new DesignationDto
        {
            Id = d.Id,
            OrganizationId = d.OrganizationId,
            Name = d.Name
        }).ToList();

        return ApiResponseDto<List<DesignationDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<DesignationDto>> GetDesignationByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var d = await _context.Designations
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (d == null) return ApiResponseDto<DesignationDto>.Fail("Designation not found.");

        var dto = new DesignationDto
        {
            Id = d.Id,
            OrganizationId = d.OrganizationId,
            Name = d.Name
        };

        return ApiResponseDto<DesignationDto>.Ok(dto);
    }

    public async Task<ApiResponseDto<DesignationDto>> CreateDesignationAsync(CreateDesignationDto request, CancellationToken cancellationToken = default)
    {
        var designation = new Designation
        {
            OrganizationId = request.OrganizationId,
            Name = request.Name.Trim()
        };

        await _context.Designations.AddAsync(designation, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetDesignationByIdAsync(designation.Id, cancellationToken);
    }

    public async Task<ApiResponseDto<bool>> UpdateDesignationAsync(int id, UpdateDesignationDto request, CancellationToken cancellationToken = default)
    {
        var designation = await _context.Designations.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (designation == null) return ApiResponseDto<bool>.Fail("Designation not found.");

        designation.Name = request.Name.Trim();

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseDto<bool>.Ok(true, "Designation updated successfully.");
    }

    public async Task<ApiResponseDto<bool>> DeleteDesignationAsync(int id, CancellationToken cancellationToken = default)
    {
        var designation = await _context.Designations.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (designation == null) return ApiResponseDto<bool>.Fail("Designation not found.");

        designation.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseDto<bool>.Ok(true, "Designation deleted successfully.");
    }
}

public class LocationService : ILocationService
{
    private readonly ApplicationDbContext _context;

    public LocationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponseDto<List<LocationDto>>> GetLocationsAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        var locations = await _context.Locations
            .AsNoTracking()
            .Where(l => l.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        var dtos = locations.Select(l => new LocationDto
        {
            Id = l.Id,
            OrganizationId = l.OrganizationId,
            Name = l.Name,
            Country = l.Country,
            EmailAlias = l.EmailAlias,
            Latitude = l.Latitude,
            Longitude = l.Longitude,
            Radius = l.Radius,
            EnforceGeofence = l.EnforceGeofence,
            TimeZoneValue = l.TimeZoneValue
        }).ToList();

        return ApiResponseDto<List<LocationDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<LocationDto>> GetLocationByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var l = await _context.Locations
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        if (l == null) return ApiResponseDto<LocationDto>.Fail("Location not found.");

        var dto = new LocationDto
        {
            Id = l.Id,
            OrganizationId = l.OrganizationId,
            Name = l.Name,
            Country = l.Country,
            EmailAlias = l.EmailAlias,
            Latitude = l.Latitude,
            Longitude = l.Longitude,
            Radius = l.Radius,
            EnforceGeofence = l.EnforceGeofence,
            TimeZoneValue = l.TimeZoneValue
        };

        return ApiResponseDto<LocationDto>.Ok(dto);
    }

    public async Task<ApiResponseDto<LocationDto>> CreateLocationAsync(CreateLocationDto request, CancellationToken cancellationToken = default)
    {
        var location = new Location
        {
            OrganizationId = request.OrganizationId,
            Name = request.Name.Trim(),
            Country = request.Country?.Trim(),
            EmailAlias = request.EmailAlias?.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Radius = request.Radius,
            EnforceGeofence = request.EnforceGeofence,
            TimeZoneValue = request.TimeZoneValue?.Trim()
        };

        await _context.Locations.AddAsync(location, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetLocationByIdAsync(location.Id, cancellationToken);
    }

    public async Task<ApiResponseDto<bool>> UpdateLocationAsync(int id, UpdateLocationDto request, CancellationToken cancellationToken = default)
    {
        var location = await _context.Locations.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (location == null) return ApiResponseDto<bool>.Fail("Location not found.");

        location.Name = request.Name.Trim();
        location.Country = request.Country?.Trim();
        location.EmailAlias = request.EmailAlias?.Trim();
        location.Latitude = request.Latitude;
        location.Longitude = request.Longitude;
        location.Radius = request.Radius;
        if (request.EnforceGeofence.HasValue)
        {
            location.EnforceGeofence = request.EnforceGeofence.Value;
        }
        location.TimeZoneValue = request.TimeZoneValue?.Trim();

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseDto<bool>.Ok(true, "Location updated successfully.");
    }


    public async Task<ApiResponseDto<bool>> DeleteLocationAsync(int id, CancellationToken cancellationToken = default)
    {
        var location = await _context.Locations.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (location == null) return ApiResponseDto<bool>.Fail("Location not found.");

        location.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseDto<bool>.Ok(true, "Location deleted successfully.");
    }
}

public class AcademicYearService : IAcademicYearService
{
    private readonly ApplicationDbContext _context;

    public AcademicYearService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponseDto<List<AcademicYearDto>>> GetAcademicYearsAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        var years = await _context.AcademicYears
            .AsNoTracking()
            .Where(y => y.OrganizationId == organizationId)
            .OrderByDescending(y => y.StartDate)
            .ToListAsync(cancellationToken);

        var dtos = years.Select(y => new AcademicYearDto
        {
            Id = y.Id,
            OrganizationId = y.OrganizationId,
            StartDate = y.StartDate,
            EndDate = y.EndDate,
            IsActive = y.IsActive
        }).ToList();

        return ApiResponseDto<List<AcademicYearDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<AcademicYearDto>> GetAcademicYearByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var y = await _context.AcademicYears
            .AsNoTracking()
            .FirstOrDefaultAsync(y => y.Id == id, cancellationToken);

        if (y == null) return ApiResponseDto<AcademicYearDto>.Fail("Academic Year not found.");

        var dto = new AcademicYearDto
        {
            Id = y.Id,
            OrganizationId = y.OrganizationId,
            StartDate = y.StartDate,
            EndDate = y.EndDate,
            IsActive = y.IsActive
        };

        return ApiResponseDto<AcademicYearDto>.Ok(dto);
    }

    public async Task<ApiResponseDto<AcademicYearDto>> CreateAcademicYearAsync(CreateAcademicYearDto request, CancellationToken cancellationToken = default)
    {
        if (request.IsActive)
        {
            var existingActives = await _context.AcademicYears
                .Where(y => y.OrganizationId == request.OrganizationId && y.IsActive)
                .ToListAsync(cancellationToken);
            foreach (var ea in existingActives) ea.IsActive = false;
        }

        var year = new AcademicYear
        {
            OrganizationId = request.OrganizationId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive
        };

        await _context.AcademicYears.AddAsync(year, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetAcademicYearByIdAsync(year.Id, cancellationToken);
    }

    public async Task<ApiResponseDto<bool>> UpdateAcademicYearAsync(int id, UpdateAcademicYearDto request, CancellationToken cancellationToken = default)
    {
        var year = await _context.AcademicYears.FirstOrDefaultAsync(y => y.Id == id, cancellationToken);
        if (year == null) return ApiResponseDto<bool>.Fail("Academic Year not found.");

        if (request.IsActive && !year.IsActive)
        {
            var existingActives = await _context.AcademicYears
                .Where(y => y.OrganizationId == year.OrganizationId && y.IsActive)
                .ToListAsync(cancellationToken);
            foreach (var ea in existingActives) ea.IsActive = false;
        }

        year.StartDate = request.StartDate;
        year.EndDate = request.EndDate;
        year.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseDto<bool>.Ok(true, "Academic Year updated successfully.");
    }

    public async Task<ApiResponseDto<bool>> DeleteAcademicYearAsync(int id, CancellationToken cancellationToken = default)
    {
        var year = await _context.AcademicYears.FirstOrDefaultAsync(y => y.Id == id, cancellationToken);
        if (year == null) return ApiResponseDto<bool>.Fail("Academic Year not found.");

        year.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseDto<bool>.Ok(true, "Academic Year deleted successfully.");
    }
}

public class OffDayService : IOffDayService
{
    private readonly ApplicationDbContext _context;

    public OffDayService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponseDto<List<OffDayDto>>> GetOffDaysAsync(int organizationId, int academicYearId, CancellationToken cancellationToken = default)
    {
        var offDays = await _context.OffDays
            .AsNoTracking()
            .Include(o => o.Location)
            .Include(o => o.Role)
            .Where(o => o.OrganizationId == organizationId && o.AcademicYearId == academicYearId)
            .ToListAsync(cancellationToken);

        var dtos = offDays.Select(o => new OffDayDto
        {
            Id = o.Id,
            OrganizationId = o.OrganizationId,
            AcademicYearId = o.AcademicYearId,
            LocationId = o.LocationId,
            LocationName = o.Location?.Name,
            RoleId = o.RoleId,
            RoleName = o.Role?.Name,
            OffDayName = o.OffDayName,
            WorkDayType = o.WorkDayType,
            Week1 = o.Week1,
            Week2 = o.Week2,
            Week3 = o.Week3,
            Week4 = o.Week4,
            Week5 = o.Week5,
            Week6 = o.Week6
        }).ToList();

        return ApiResponseDto<List<OffDayDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<OffDayDto>> GetOffDayByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var o = await _context.OffDays
            .AsNoTracking()
            .Include(o => o.Location)
            .Include(o => o.Role)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (o == null) return ApiResponseDto<OffDayDto>.Fail("Off Day not found.");

        var dto = new OffDayDto
        {
            Id = o.Id,
            OrganizationId = o.OrganizationId,
            AcademicYearId = o.AcademicYearId,
            LocationId = o.LocationId,
            LocationName = o.Location?.Name,
            RoleId = o.RoleId,
            RoleName = o.Role?.Name,
            OffDayName = o.OffDayName,
            WorkDayType = o.WorkDayType,
            Week1 = o.Week1,
            Week2 = o.Week2,
            Week3 = o.Week3,
            Week4 = o.Week4,
            Week5 = o.Week5,
            Week6 = o.Week6
        };

        return ApiResponseDto<OffDayDto>.Ok(dto);
    }

    public async Task<ApiResponseDto<OffDayDto>> CreateOffDayAsync(CreateOffDayDto request, CancellationToken cancellationToken = default)
    {
        var offDay = new OffDay
        {
            OrganizationId = request.OrganizationId,
            AcademicYearId = request.AcademicYearId,
            LocationId = request.LocationId,
            RoleId = request.RoleId,
            OffDayName = request.OffDayName.Trim(),
            WorkDayType = request.WorkDayType,
            Week1 = request.Week1,
            Week2 = request.Week2,
            Week3 = request.Week3,
            Week4 = request.Week4,
            Week5 = request.Week5,
            Week6 = request.Week6
        };

        await _context.OffDays.AddAsync(offDay, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetOffDayByIdAsync(offDay.Id, cancellationToken);
    }

    public async Task<ApiResponseDto<bool>> UpdateOffDayAsync(int id, UpdateOffDayDto request, CancellationToken cancellationToken = default)
    {
        var offDay = await _context.OffDays.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (offDay == null) return ApiResponseDto<bool>.Fail("Off Day not found.");

        offDay.AcademicYearId = request.AcademicYearId;
        offDay.LocationId = request.LocationId;
        offDay.RoleId = request.RoleId;
        offDay.OffDayName = request.OffDayName.Trim();
        offDay.WorkDayType = request.WorkDayType;
        offDay.Week1 = request.Week1;
        offDay.Week2 = request.Week2;
        offDay.Week3 = request.Week3;
        offDay.Week4 = request.Week4;
        offDay.Week5 = request.Week5;
        offDay.Week6 = request.Week6;

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseDto<bool>.Ok(true, "Off Day updated successfully.");
    }

    public async Task<ApiResponseDto<bool>> DeleteOffDayAsync(int id, CancellationToken cancellationToken = default)
    {
        var offDay = await _context.OffDays.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (offDay == null) return ApiResponseDto<bool>.Fail("Off Day not found.");

        offDay.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseDto<bool>.Ok(true, "Off Day deleted successfully.");
    }

    public async Task<ApiResponseDto<List<OffDayDto>>> SaveOffDayMatrixAsync(SaveOffDayMatrixDto request, CancellationToken cancellationToken = default)
    {
        if (request.RoleId <= 0) return ApiResponseDto<List<OffDayDto>>.Fail("Valid Role ID is required.");
        if (request.LocationId <= 0) return ApiResponseDto<List<OffDayDto>>.Fail("Valid Location ID is required.");

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);
        if (role == null) return ApiResponseDto<List<OffDayDto>>.Fail("Role not found.");

        var location = await _context.Locations.FirstOrDefaultAsync(l => l.Id == request.LocationId, cancellationToken);
        if (location == null) return ApiResponseDto<List<OffDayDto>>.Fail("Location not found.");

        var existingRecords = await _context.OffDays
            .IgnoreQueryFilters()
            .Where(o => o.OrganizationId == request.OrganizationId
                     && o.AcademicYearId == request.AcademicYearId
                     && o.LocationId == request.LocationId
                     && o.RoleId == request.RoleId)
            .ToListAsync(cancellationToken);

        foreach (var dayItem in request.Days)
        {
            var dayName = dayItem.OffDayName.Trim();
            if (string.IsNullOrWhiteSpace(dayName)) continue;

            var existing = existingRecords.FirstOrDefault(o => o.OffDayName.Equals(dayName, StringComparison.OrdinalIgnoreCase));

            bool hasAnyWeek = dayItem.Week1 || dayItem.Week2 || dayItem.Week3 || dayItem.Week4 || dayItem.Week5 || dayItem.Week6;
            bool isOffDay = (dayItem.WorkDayType == 1 || dayItem.WorkDayType == 2) && hasAnyWeek;

            if (isOffDay)
            {
                if (existing != null)
                {
                    existing.WorkDayType = dayItem.WorkDayType;
                    existing.Week1 = dayItem.Week1;
                    existing.Week2 = dayItem.Week2;
                    existing.Week3 = dayItem.Week3;
                    existing.Week4 = dayItem.Week4;
                    existing.Week5 = dayItem.Week5;
                    existing.Week6 = dayItem.Week6;
                    existing.IsDeleted = false;
                }
                else
                {
                    var newOffDay = new OffDay
                    {
                        OrganizationId = request.OrganizationId,
                        AcademicYearId = request.AcademicYearId,
                        LocationId = request.LocationId,
                        RoleId = request.RoleId,
                        OffDayName = dayName,
                        WorkDayType = dayItem.WorkDayType,
                        Week1 = dayItem.Week1,
                        Week2 = dayItem.Week2,
                        Week3 = dayItem.Week3,
                        Week4 = dayItem.Week4,
                        Week5 = dayItem.Week5,
                        Week6 = dayItem.Week6,
                        IsDeleted = false
                    };
                    await _context.OffDays.AddAsync(newOffDay, cancellationToken);
                }
            }
            else
            {
                if (existing != null && !existing.IsDeleted)
                {
                    existing.IsDeleted = true;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return await GetOffDaysAsync(request.OrganizationId, request.AcademicYearId, cancellationToken);
    }

    public async Task<ApiResponseDto<List<OffDayDto>>> GetOffDayMatrixAsync(int organizationId, int academicYearId, int? roleId = null, int? locationId = null, bool includeWorkingDays = true, CancellationToken cancellationToken = default)
    {
        var query = _context.OffDays
            .AsNoTracking()
            .Include(o => o.Location)
            .Include(o => o.Role)
            .Where(o => o.OrganizationId == organizationId && o.AcademicYearId == academicYearId);

        if (roleId.HasValue && roleId.Value > 0)
        {
            query = query.Where(o => o.RoleId == roleId.Value);
        }

        if (locationId.HasValue && locationId.Value > 0)
        {
            query = query.Where(o => o.LocationId == locationId.Value);
        }

        var existingOffDays = await query.ToListAsync(cancellationToken);

        if (!includeWorkingDays)
        {
            var rawDtos = existingOffDays.Select(o => new OffDayDto
            {
                Id = o.Id,
                OrganizationId = o.OrganizationId,
                AcademicYearId = o.AcademicYearId,
                LocationId = o.LocationId,
                LocationName = o.Location?.Name,
                RoleId = o.RoleId,
                RoleName = o.Role?.Name,
                OffDayName = o.OffDayName,
                WorkDayType = o.WorkDayType,
                Week1 = o.Week1,
                Week2 = o.Week2,
                Week3 = o.Week3,
                Week4 = o.Week4,
                Week5 = o.Week5,
                Week6 = o.Week6
            }).ToList();
            return ApiResponseDto<List<OffDayDto>>.Ok(rawDtos);
        }

        var weekdays = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        var resultDtos = new List<OffDayDto>();

        if (roleId.HasValue && locationId.HasValue)
        {
            var role = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roleId.Value, cancellationToken);
            var location = await _context.Locations.AsNoTracking().FirstOrDefaultAsync(l => l.Id == locationId.Value, cancellationToken);

            foreach (var day in weekdays)
            {
                var match = existingOffDays.FirstOrDefault(o => o.OffDayName.Equals(day, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    resultDtos.Add(new OffDayDto
                    {
                        Id = match.Id,
                        OrganizationId = match.OrganizationId,
                        AcademicYearId = match.AcademicYearId,
                        LocationId = match.LocationId,
                        LocationName = match.Location?.Name ?? location?.Name,
                        RoleId = match.RoleId,
                        RoleName = match.Role?.Name ?? role?.Name,
                        OffDayName = match.OffDayName,
                        WorkDayType = match.WorkDayType,
                        Week1 = match.Week1,
                        Week2 = match.Week2,
                        Week3 = match.Week3,
                        Week4 = match.Week4,
                        Week5 = match.Week5,
                        Week6 = match.Week6
                    });
                }
                else
                {
                    resultDtos.Add(new OffDayDto
                    {
                        Id = 0,
                        OrganizationId = organizationId,
                        AcademicYearId = academicYearId,
                        LocationId = locationId.Value,
                        LocationName = location?.Name,
                        RoleId = roleId.Value,
                        RoleName = role?.Name,
                        OffDayName = day,
                        WorkDayType = 0, // 0: Working Day
                        Week1 = false,
                        Week2 = false,
                        Week3 = false,
                        Week4 = false,
                        Week5 = false,
                        Week6 = false
                    });
                }
            }
        }
        else
        {
            var roles = await _context.Roles.AsNoTracking().Where(r => r.OrganizationId == organizationId).ToListAsync(cancellationToken);
            var locations = await _context.Locations.AsNoTracking().Where(l => l.OrganizationId == organizationId).ToListAsync(cancellationToken);

            foreach (var r in roles)
            {
                foreach (var loc in locations)
                {
                    var pairOffDays = existingOffDays.Where(o => o.RoleId == r.Id && o.LocationId == loc.Id).ToList();
                    if (pairOffDays.Any() || (roleId.HasValue && roleId.Value == r.Id))
                    {
                        foreach (var day in weekdays)
                        {
                            var match = pairOffDays.FirstOrDefault(o => o.OffDayName.Equals(day, StringComparison.OrdinalIgnoreCase));
                            if (match != null)
                            {
                                resultDtos.Add(new OffDayDto
                                {
                                    Id = match.Id,
                                    OrganizationId = match.OrganizationId,
                                    AcademicYearId = match.AcademicYearId,
                                    LocationId = match.LocationId,
                                    LocationName = loc.Name,
                                    RoleId = r.Id,
                                    RoleName = r.Name,
                                    OffDayName = match.OffDayName,
                                    WorkDayType = match.WorkDayType,
                                    Week1 = match.Week1,
                                    Week2 = match.Week2,
                                    Week3 = match.Week3,
                                    Week4 = match.Week4,
                                    Week5 = match.Week5,
                                    Week6 = match.Week6
                                });
                            }
                            else
                            {
                                resultDtos.Add(new OffDayDto
                                {
                                    Id = 0,
                                    OrganizationId = organizationId,
                                    AcademicYearId = academicYearId,
                                    LocationId = loc.Id,
                                    LocationName = loc.Name,
                                    RoleId = r.Id,
                                    RoleName = r.Name,
                                    OffDayName = day,
                                    WorkDayType = 0, // 0: Working Day
                                    Week1 = false,
                                    Week2 = false,
                                    Week3 = false,
                                    Week4 = false,
                                    Week5 = false,
                                    Week6 = false
                                });
                            }
                        }
                    }
                }
            }
        }

        return ApiResponseDto<List<OffDayDto>>.Ok(resultDtos);
    }
}

public class LeaveTypeService : ILeaveTypeService
{
    private readonly ApplicationDbContext _context;

    public LeaveTypeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponseDto<List<LeaveTypeDto>>> GetLeaveTypesAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        var leaveTypes = await _context.LeaveTypes
            .AsNoTracking()
            .Include(lt => lt.LeaveSetting)
            .Where(lt => lt.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        var dtos = leaveTypes.Select(lt => new LeaveTypeDto
        {
            Id = lt.Id,
            OrganizationId = lt.OrganizationId,
            Name = lt.Name,
            Description = lt.Description,
            IsActive = lt.IsActive,
            Setting = lt.LeaveSetting != null ? new LeaveSettingDto
            {
                Id = lt.LeaveSetting.Id,
                IsPaid = lt.LeaveSetting.IsPaid,
                Leaves = lt.LeaveSetting.Leaves ?? 0,
                CanTakeHalfDay = lt.LeaveSetting.CanTakeHalfDay,
                CarryForwardLeaveCount = lt.LeaveSetting.CarryForwardLeaveCount ?? 0
            } : null
        }).ToList();

        return ApiResponseDto<List<LeaveTypeDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<LeaveTypeDto>> GetLeaveTypeByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var lt = await _context.LeaveTypes
            .AsNoTracking()
            .Include(x => x.LeaveSetting)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (lt == null) return ApiResponseDto<LeaveTypeDto>.Fail("Leave Type not found.");

        var dto = new LeaveTypeDto
        {
            Id = lt.Id,
            OrganizationId = lt.OrganizationId,
            Name = lt.Name,
            Description = lt.Description,
            IsActive = lt.IsActive,
            Setting = lt.LeaveSetting != null ? new LeaveSettingDto
            {
                Id = lt.LeaveSetting.Id,
                IsPaid = lt.LeaveSetting.IsPaid,
                Leaves = lt.LeaveSetting.Leaves ?? 0,
                CanTakeHalfDay = lt.LeaveSetting.CanTakeHalfDay,
                CarryForwardLeaveCount = lt.LeaveSetting.CarryForwardLeaveCount ?? 0
            } : null
        };

        return ApiResponseDto<LeaveTypeDto>.Ok(dto);
    }

    public async Task<ApiResponseDto<LeaveTypeDto>> CreateLeaveTypeAsync(CreateLeaveTypeDto request, CancellationToken cancellationToken = default)
    {
        var leaveType = new LeaveType
        {
            OrganizationId = request.OrganizationId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            IsActive = request.IsActive,
            LeaveSetting = new LeaveSetting
            {
                OrganizationId = request.OrganizationId,
                IsPaid = request.IsPaid,
                Leaves = request.Leaves,
                CanTakeHalfDay = request.CanTakeHalfDay,
                CarryForwardLeaveCount = request.CarryForwardLeaveCount
            }
        };

        await _context.LeaveTypes.AddAsync(leaveType, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetLeaveTypeByIdAsync(leaveType.Id, cancellationToken);
    }

    public async Task<ApiResponseDto<bool>> UpdateLeaveTypeAsync(int id, UpdateLeaveTypeDto request, CancellationToken cancellationToken = default)
    {
        var leaveType = await _context.LeaveTypes
            .Include(lt => lt.LeaveSetting)
            .FirstOrDefaultAsync(lt => lt.Id == id, cancellationToken);

        if (leaveType == null) return ApiResponseDto<bool>.Fail("Leave Type not found.");

        leaveType.Name = request.Name.Trim();
        leaveType.Description = request.Description?.Trim();
        leaveType.IsActive = request.IsActive;

        if (leaveType.LeaveSetting != null)
        {
            leaveType.LeaveSetting.IsPaid = request.IsPaid;
            leaveType.LeaveSetting.Leaves = request.Leaves;
            leaveType.LeaveSetting.CanTakeHalfDay = request.CanTakeHalfDay;
            leaveType.LeaveSetting.CarryForwardLeaveCount = request.CarryForwardLeaveCount;
        }
        else
        {
            leaveType.LeaveSetting = new LeaveSetting
            {
                OrganizationId = leaveType.OrganizationId,
                LeaveTypeId = leaveType.Id,
                IsPaid = request.IsPaid,
                Leaves = request.Leaves,
                CanTakeHalfDay = request.CanTakeHalfDay,
                CarryForwardLeaveCount = request.CarryForwardLeaveCount
            };
            await _context.LeaveSettings.AddAsync(leaveType.LeaveSetting, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseDto<bool>.Ok(true, "Leave Type updated successfully.");
    }

    public async Task<ApiResponseDto<bool>> DeleteLeaveTypeAsync(int id, CancellationToken cancellationToken = default)
    {
        var leaveType = await _context.LeaveTypes.FirstOrDefaultAsync(lt => lt.Id == id, cancellationToken);
        if (leaveType == null) return ApiResponseDto<bool>.Fail("Leave Type not found.");

        leaveType.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseDto<bool>.Ok(true, "Leave Type deleted successfully.");
    }
}

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponseDto<List<NotificationDto>>> GetNotificationsAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var notifications = await _context.Notifications
            .AsNoTracking()
            .Where(n => n.EmployeeId == employeeId)
            .OrderByDescending(n => n.Date)
            .Take(50)
            .ToListAsync(cancellationToken);

        var dtos = notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            OrganizationId = n.OrganizationId,
            EmployeeId = n.EmployeeId,
            NotificationHeader = n.NotificationHeader,
            Message = n.Message,
            NotificationType = n.NotificationType,
            Date = n.Date,
            ReadStatus = n.ReadStatus,
            Url = n.Url
        }).ToList();

        return ApiResponseDto<List<NotificationDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<bool>> MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId, cancellationToken);
        if (notification != null)
        {
            notification.ReadStatus = true;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return ApiResponseDto<bool>.Ok(true);
    }
}
