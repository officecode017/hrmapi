using Microsoft.EntityFrameworkCore;
using HRAttendance.Business.Interfaces;
using HRAttendance.Data;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Shift;
using HRAttendance.Data.DTOs.Holiday;
using HRAttendance.Data.DTOs.Role;
using HRAttendance.Data.DTOs.Notification;
using HRAttendance.Data.DTOs.Overtime;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Security;
using HRAttendance.Data.Models.Overtime;

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
            .Where(s => s.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        var dtos = shifts.Select(s => new ShiftDto
        {
            Id = s.Id,
            OrganizationId = s.OrganizationId,
            LocationId = s.LocationId,
            Name = s.Name,
            InTime = s.InTime,
            OutTime = s.OutTime,
            IsOvernight = s.IsOvernight,
            GraceMinutes = s.GraceMinutes,
            BreakMinutes = s.BreakMinutes
        }).ToList();

        return ApiResponseDto<List<ShiftDto>>.Ok(dtos);
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
            BreakMinutes = request.BreakMinutes
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
            BreakMinutes = shift.BreakMinutes
        };

        return ApiResponseDto<ShiftDto>.Ok(dto, "Shift created successfully.");
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
            .Where(h => h.OrganizationId == organizationId && h.AcademicYearId == academicYearId)
            .OrderBy(h => h.Date)
            .ToListAsync(cancellationToken);

        var dtos = holidays.Select(h => new HolidayDto
        {
            Id = h.Id,
            OrganizationId = h.OrganizationId,
            LocationId = h.LocationId,
            AcademicYearId = h.AcademicYearId,
            Name = h.Name,
            Date = h.Date,
            IsOptional = h.IsOptional
        }).ToList();

        return ApiResponseDto<List<HolidayDto>>.Ok(dtos);
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

        var dto = new HolidayDto
        {
            Id = holiday.Id,
            OrganizationId = holiday.OrganizationId,
            LocationId = holiday.LocationId,
            AcademicYearId = holiday.AcademicYearId,
            Name = holiday.Name,
            Date = holiday.Date,
            IsOptional = holiday.IsOptional
        };

        return ApiResponseDto<HolidayDto>.Ok(dto, "Holiday created successfully.");
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
            .Where(r => r.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        var dtos = roles.Select(r => new RoleDto
        {
            Id = r.Id,
            OrganizationId = r.OrganizationId,
            Name = r.Name,
            Description = r.Description,
            IsActive = r.IsActive
        }).ToList();

        return ApiResponseDto<List<RoleDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<bool>> AssignRoleAsync(AssignRoleDto request, CancellationToken cancellationToken = default)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);
        if (employee == null) return ApiResponseDto<bool>.Fail("Employee not found.");

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);
        if (role == null) return ApiResponseDto<bool>.Fail("Role not found.");

        var exists = await _context.EmployeeRoles.AnyAsync(er => er.EmployeeId == request.EmployeeId && er.RoleId == request.RoleId, cancellationToken);
        if (!exists)
        {
            await _context.EmployeeRoles.AddAsync(new EmployeeRole
            {
                OrganizationId = employee.OrganizationId,
                EmployeeId = request.EmployeeId,
                RoleId = request.RoleId
            }, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }

        return ApiResponseDto<bool>.Ok(true, "Role assigned successfully.");
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

public class OvertimeService : IOvertimeService
{
    private readonly ApplicationDbContext _context;

    public OvertimeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponseDto<List<OTEntryDto>>> GetEntriesAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var entries = await _context.OTEntries
            .AsNoTracking()
            .Where(o => o.EmployeeId == employeeId)
            .Include(o => o.Employee)
            .OrderByDescending(o => o.OTDate)
            .ToListAsync(cancellationToken);

        var dtos = entries.Select(o => new OTEntryDto
        {
            Id = o.Id,
            OrganizationId = o.OrganizationId,
            EmployeeId = o.EmployeeId,
            EmployeeName = $"{o.Employee.FirstName} {o.Employee.LastName}".Trim(),
            OTDate = o.OTDate,
            OTHours = o.OTHours,
            MultiplierApplied = o.MultiplierApplied,
            HourlyRate = o.HourlyRate,
            OTAmount = o.OTAmount
        }).ToList();

        return ApiResponseDto<List<OTEntryDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<OTEntryDto>> CalculateOvertimeAsync(CalculateOvertimeRequestDto request, CancellationToken cancellationToken = default)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);
        if (employee == null) return ApiResponseDto<OTEntryDto>.Fail("Employee not found.");

        var setting = await _context.OTSettings.FirstOrDefaultAsync(s => s.OrganizationId == employee.OrganizationId && s.IsActive, cancellationToken);
        if (setting == null || !setting.IsOverTimeEnabled)
        {
            return ApiResponseDto<OTEntryDto>.Fail("Overtime calculation is not enabled for this organization.");
        }

        var standardDailyHours = 8.0m;
        var otHours = Math.Max(0, request.HoursWorked - standardDailyHours);
        if (setting.MaxOTHoursPerDay.HasValue && otHours > setting.MaxOTHoursPerDay.Value)
        {
            otHours = setting.MaxOTHoursPerDay.Value;
        }

        var hourlyRate = 100.00m; // Default baseline rate
        var multiplier = setting.Multiplier > 0 ? setting.Multiplier : 1.5m;
        var totalAmount = otHours * hourlyRate * multiplier;

        var entry = new OTEntry
        {
            OrganizationId = employee.OrganizationId,
            EmployeeId = employee.Id,
            OTSettingId = setting.Id,
            OTDate = request.Date,
            OTHours = otHours,
            MultiplierApplied = multiplier,
            HourlyRate = hourlyRate,
            OTAmount = totalAmount
        };

        await _context.OTEntries.AddAsync(entry, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new OTEntryDto
        {
            Id = entry.Id,
            OrganizationId = entry.OrganizationId,
            EmployeeId = entry.EmployeeId,
            EmployeeName = $"{employee.FirstName} {employee.LastName}".Trim(),
            OTDate = entry.OTDate,
            OTHours = entry.OTHours,
            MultiplierApplied = entry.MultiplierApplied,
            HourlyRate = entry.HourlyRate,
            OTAmount = entry.OTAmount
        };

        return ApiResponseDto<OTEntryDto>.Ok(dto, "Overtime calculated and recorded successfully.");
    }
}
