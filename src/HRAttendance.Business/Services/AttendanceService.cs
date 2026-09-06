using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HRAttendance.Business.BusinessRules;
using HRAttendance.Business.Interfaces;
using HRAttendance.Data;
using HRAttendance.Data.DTOs.Attendance;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.Interfaces;
using HRAttendance.Data.Models.Attendance;
using HRAttendance.Data.Models.Overtime;

namespace HRAttendance.Business.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AttendanceService> _logger;

    public AttendanceService(
        IAttendanceRepository attendanceRepository,
        ApplicationDbContext context,
        ILogger<AttendanceService> logger)
    {
        _attendanceRepository = attendanceRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponseDto<AttendanceDto>> CheckInAsync(CheckInRequestDto request, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var existing = await _attendanceRepository.GetTodayAttendanceAsync(request.EmployeeId, today, cancellationToken);
        if (existing != null)
        {
            return ApiResponseDto<AttendanceDto>.Fail("You have already checked in today.");
        }

        var employee = await _context.Employees
            .Include(e => e.ProfessionalDetails)
            .Include(e => e.EmployeeRoles)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
        {
            return ApiResponseDto<AttendanceDto>.Fail("Employee not found.");
        }

        var now = DateTimeOffset.UtcNow;

        // 1. BUSINESS RULE: Check if employee is on Approved Leave for today
        var onApprovedLeave = await _context.LeaveApplications
            .AnyAsync(l => l.EmployeeId == request.EmployeeId &&
                           l.Status == LeaveStatus.Approved &&
                           l.LeaveFrom <= now &&
                           l.LeaveTo >= now, cancellationToken);

        if (onApprovedLeave)
        {
            return ApiResponseDto<AttendanceDto>.Fail("Cannot check in: You have an approved leave scheduled for today.");
        }

        // 2. Resolve Shift
        var shiftId = employee.ProfessionalDetails?.ShiftId;
        var shift = shiftId.HasValue 
            ? await _context.Shifts.FirstOrDefaultAsync(s => s.Id == shiftId.Value, cancellationToken)
            : await _context.Shifts.FirstOrDefaultAsync(s => s.OrganizationId == employee.OrganizationId, cancellationToken);

        if (shift == null)
        {
            return ApiResponseDto<AttendanceDto>.Fail("No shift configuration found for employee or organization.");
        }

        var academicYear = await _context.AcademicYears
            .FirstOrDefaultAsync(a => a.OrganizationId == employee.OrganizationId && a.IsActive, cancellationToken);

        if (academicYear == null)
        {
            return ApiResponseDto<AttendanceDto>.Fail("No active academic year configured for organization.");
        }

        var targetLocationId = request.LocationId > 0 ? request.LocationId : (employee.ProfessionalDetails?.LocationId ?? shift.LocationId);
        var location = await _context.Locations.FirstOrDefaultAsync(l => l.Id == targetLocationId, cancellationToken);

        // 3. BUSINESS RULE: Geofence Validation (Optional / Configurable per campus location)
        if (location != null && location.EnforceGeofence && location.Latitude.HasValue && location.Longitude.HasValue && location.Radius.HasValue && location.Radius.Value > 0)
        {
            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                var distanceMeters = CalculateDistanceMeters(
                    (double)location.Latitude.Value, (double)location.Longitude.Value,
                    (double)request.Latitude.Value, (double)request.Longitude.Value);

                if (distanceMeters > location.Radius.Value)
                {
                    return ApiResponseDto<AttendanceDto>.Fail($"Check-in rejected: Location is outside the allowed geofence ({Math.Round(distanceMeters)}m from campus, allowed radius is {location.Radius.Value}m).");
                }
            }
        }


        // 4. BUSINESS RULE: Check if today is an official Holiday
        string? dynamicRemark = request.Remark;
        var holiday = await _context.Holidays
            .FirstOrDefaultAsync(h => h.OrganizationId == employee.OrganizationId &&
                                      h.Date == today &&
                                      (h.LocationId == null || h.LocationId == targetLocationId), cancellationToken);
        if (holiday != null)
        {
            dynamicRemark = string.IsNullOrEmpty(dynamicRemark) 
                ? $"Worked on Holiday: {holiday.Name}" 
                : $"{dynamicRemark} [Worked on Holiday: {holiday.Name}]";
        }

        // 5. BUSINESS RULE: Check if today is an assigned Off-Day / Weekend
        var primaryRoleId = employee.EmployeeRoles.FirstOrDefault()?.RoleId ?? 0;
        var dayOfWeek = DateTime.UtcNow.DayOfWeek;
        var offDay = await _context.OffDays
            .FirstOrDefaultAsync(o => o.OrganizationId == employee.OrganizationId &&
                                      o.AcademicYearId == academicYear.Id &&
                                      (o.LocationId == targetLocationId || o.LocationId == 0) &&
                                      (o.RoleId == primaryRoleId || o.RoleId == 0) &&
                                      o.OffDayName.ToLower().Contains(dayOfWeek.ToString().ToLower()), cancellationToken);
        if (offDay != null)
        {
            dynamicRemark = string.IsNullOrEmpty(dynamicRemark) 
                ? "Worked on Off-Day" 
                : $"{dynamicRemark} [Worked on Off-Day]";
        }

        var currentTime = TimeOnly.FromTimeSpan(now.TimeOfDay);

        // Calculate Late arrival
        var shiftGraceThreshold = shift.InTime.AddMinutes(shift.GraceMinutes);
        var status = currentTime > shiftGraceThreshold ? (int)AttendanceStatus.Late : (int)AttendanceStatus.Present;

        var attendance = new EmployeeAttendance
        {
            OrganizationId = employee.OrganizationId,
            EmployeeId = request.EmployeeId,
            LocationId = targetLocationId,
            ShiftId = shift.Id,
            AcademicYearId = academicYear.Id,
            InTime = now,
            InTimeLatitude = request.Latitude,
            InTimeLongitude = request.Longitude,
            CheckInMacId = request.MacId,
            InNetworkSource = request.InNetworkSource,
            InPlatform = request.InPlatform,
            AppVersion = request.AppVersion,
            Remark = dynamicRemark,
            Status = status,
            PunchCount = 1
        };

        await _attendanceRepository.AddAsync(attendance, cancellationToken);
        _logger.LogInformation("Employee {EmployeeId} checked in with status {Status} at {InTime}", request.EmployeeId, (AttendanceStatus)status, now);

        return await MapToDtoAsync(attendance.Id, cancellationToken);
    }

    public async Task<ApiResponseDto<AttendanceDto>> CheckOutAsync(CheckOutRequestDto request, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var attendance = await _attendanceRepository.GetTodayAttendanceAsync(request.EmployeeId, today, cancellationToken);
        if (attendance == null)
        {
            return ApiResponseDto<AttendanceDto>.Fail("No check-in record found for today.");
        }

        if (attendance.OutTime.HasValue)
        {
            return ApiResponseDto<AttendanceDto>.Fail("You have already checked out today.");
        }

        var now = DateTimeOffset.UtcNow;
        attendance.OutTime = now;
        attendance.OutTimeLatitude = request.Latitude;
        attendance.OutTimeLongitude = request.Longitude;
        attendance.CheckOutMacId = request.MacId;
        attendance.OutNetworkSource = request.OutNetworkSource;
        attendance.OutPlatform = request.OutPlatform;

        if (!string.IsNullOrEmpty(request.Remark))
        {
            attendance.Remark = string.IsNullOrEmpty(attendance.Remark) ? request.Remark : $"{attendance.Remark} | {request.Remark}";
        }

        // Calculate DayTotal in decimal hours
        if (attendance.InTime.HasValue)
        {
            var duration = now - attendance.InTime.Value;
            attendance.DayTotal = Math.Round((decimal)duration.TotalHours, 2);
        }

        attendance.PunchCount = (attendance.PunchCount ?? 1) + 1;

        // 6. BUSINESS RULE: Auto-Calculate Overtime on Checkout
        var shift = await _context.Shifts.FirstOrDefaultAsync(s => s.Id == attendance.ShiftId, cancellationToken);
        var otSetting = await _context.OTSettings
            .FirstOrDefaultAsync(o => o.OrganizationId == attendance.OrganizationId && o.IsActive && o.IsOverTimeEnabled, cancellationToken);

        if (shift != null && otSetting != null)
        {
            var currentTime = TimeOnly.FromTimeSpan(now.TimeOfDay);
            var otThresholdTime = shift.OutTime.AddMinutes(otSetting.OTStartAfterMinutes);

            if (currentTime > otThresholdTime)
            {
                var otDuration = (currentTime - shift.OutTime).TotalHours;
                var maxDaily = otSetting.MaxOTHoursPerDay ?? 4.0m;
                var otHours = Math.Min((decimal)otDuration, maxDaily);

                if (otHours > 0)
                {
                    var otEntry = new OTEntry
                    {
                        OrganizationId = attendance.OrganizationId,
                        EmployeeId = attendance.EmployeeId,
                        OTSettingId = otSetting.Id,
                        OTDate = today,
                        ActualOutTime = now,
                        OTHours = Math.Round(otHours, 2),
                        MultiplierApplied = otSetting.Multiplier,
                        HourlyRate = 0,
                        OTAmount = 0
                    };
                    await _context.OTEntries.AddAsync(otEntry, cancellationToken);
                    attendance.Remark = $"{attendance.Remark} [OT Auto-Logged: {Math.Round(otHours, 2)} hrs]".Trim();
                }
            }
        }

        await _attendanceRepository.UpdateAsync(attendance, cancellationToken);
        _logger.LogInformation("Employee {EmployeeId} checked out. Day total: {DayTotal} hours", request.EmployeeId, attendance.DayTotal);

        return await MapToDtoAsync(attendance.Id, cancellationToken);
    }

    public async Task<ApiResponseDto<List<AttendanceHistoryDto>>> GetHistoryAsync(int employeeId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default)
    {
        var records = await _attendanceRepository.GetHistoryAsync(employeeId, fromDate, toDate, cancellationToken);

        var dtos = records.Select(a => new AttendanceHistoryDto
        {
            Id = a.Id,
            EmployeeId = a.EmployeeId,
            EmployeeName = a.Employee != null ? $"{a.Employee.FirstName} {a.Employee.LastName}".Trim() : string.Empty,
            Date = a.InTime.HasValue ? DateOnly.FromDateTime(a.InTime.Value.DateTime) : DateOnly.MinValue,
            InTime = a.InTime,
            OutTime = a.OutTime,
            DayTotal = a.DayTotal,
            Status = ((AttendanceStatus)a.Status).ToString(),
            ShiftName = a.Shift?.Name ?? string.Empty,
            LocationName = a.Location?.Name ?? string.Empty,
            Remark = a.Remark
        }).ToList();

        return ApiResponseDto<List<AttendanceHistoryDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<AttendanceDto?>> GetTodayStatusAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var attendance = await _attendanceRepository.GetTodayAttendanceAsync(employeeId, today, cancellationToken);
        if (attendance == null)
        {
            return ApiResponseDto<AttendanceDto?>.Ok(new AttendanceDto
            {
                EmployeeId = employeeId,
                Status = AttendanceStatus.Absent.ToString()
            }, "No check-in recorded for today.");
        }

        var result = await MapToDtoAsync(attendance.Id, cancellationToken);
        if (!result.Success) return ApiResponseDto<AttendanceDto?>.Fail(result.Message ?? "Attendance not found.");
        return ApiResponseDto<AttendanceDto?>.Ok(result.Data);
    }

    public async Task<ApiResponseDto<AttendanceDto>> AdminMarkAttendanceAsync(AdminMarkAttendanceRequestDto request, int adminUserId, CancellationToken cancellationToken = default)
    {
        var employee = await _context.Employees
            .Include(e => e.ProfessionalDetails)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
        {
            return ApiResponseDto<AttendanceDto>.Fail("Employee not found.");
        }

        // Parse status string to AttendanceStatus
        int statusInt = request.Status?.Trim().ToLowerInvariant() switch
        {
            "present" => (int)AttendanceStatus.Present,
            "late" => (int)AttendanceStatus.Late,
            "halfday" or "halfdayleave" => (int)AttendanceStatus.HalfDay,
            "absent" => (int)AttendanceStatus.Absent,
            "onleave" => (int)AttendanceStatus.OnLeave,
            "holiday" => (int)AttendanceStatus.Holiday,
            "weekend" or "weeklyoff" => (int)AttendanceStatus.Weekend,
            _ => (int)AttendanceStatus.Present
        };

        // Construct InTime and OutTime DateTimeOffsets
        DateTimeOffset? inDateTime = null;
        DateTimeOffset? outDateTime = null;
        decimal? dayTotal = null;

        if (request.InTime.HasValue)
        {
            inDateTime = new DateTimeOffset(request.Date.ToDateTime(request.InTime.Value), TimeSpan.Zero);
        }

        if (request.OutTime.HasValue)
        {
            outDateTime = new DateTimeOffset(request.Date.ToDateTime(request.OutTime.Value), TimeSpan.Zero);
        }

        if (inDateTime.HasValue && outDateTime.HasValue)
        {
            var duration = outDateTime.Value - inDateTime.Value;
            dayTotal = Math.Round((decimal)Math.Max(0, duration.TotalHours), 2);
        }

        // Determine if target record exists:
        // 1. By ID if specified
        // 2. Or by EmployeeId and Date
        EmployeeAttendance? attendance = null;
        if (request.AttendanceId.HasValue && request.AttendanceId.Value > 0)
        {
            attendance = await _attendanceRepository.GetByIdAsync(request.AttendanceId.Value, cancellationToken);
        }

        if (attendance == null)
        {
            attendance = await _attendanceRepository.GetTodayAttendanceAsync(request.EmployeeId, request.Date, cancellationToken);
        }

        if (attendance != null)
        {
            // Edit existing attendance record
            if (inDateTime.HasValue) attendance.InTime = inDateTime;
            if (outDateTime.HasValue) attendance.OutTime = outDateTime;
            if (dayTotal.HasValue) attendance.DayTotal = dayTotal;
            attendance.Status = statusInt;

            if (request.LocationId.HasValue && request.LocationId.Value > 0)
            {
                attendance.LocationId = request.LocationId.Value;
            }

            if (request.ShiftId.HasValue && request.ShiftId.Value > 0)
            {
                attendance.ShiftId = request.ShiftId.Value;
            }

            var remarkSuffix = !string.IsNullOrWhiteSpace(request.Remark) ? $" [Admin Adjusted: {request.Remark.Trim()}]" : " [Admin Adjusted]";
            attendance.Remark = string.IsNullOrEmpty(attendance.Remark) ? remarkSuffix.Trim() : $"{attendance.Remark} {remarkSuffix}".Trim();
            attendance.ApprovedByClientId = adminUserId;

            await _attendanceRepository.UpdateAsync(attendance, cancellationToken);
            _logger.LogInformation("Admin {AdminId} edited attendance {AttendanceId} for employee {EmployeeId} on {Date}", adminUserId, attendance.Id, request.EmployeeId, request.Date);

            return await MapToDtoAsync(attendance.Id, cancellationToken);
        }
        else
        {
            // Mark new previous attendance record
            var shiftId = request.ShiftId ?? employee.ProfessionalDetails?.ShiftId;
            var shift = shiftId.HasValue
                ? await _context.Shifts.FirstOrDefaultAsync(s => s.Id == shiftId.Value, cancellationToken)
                : await _context.Shifts.FirstOrDefaultAsync(s => s.OrganizationId == employee.OrganizationId, cancellationToken);

            if (shift == null)
            {
                return ApiResponseDto<AttendanceDto>.Fail("No shift configuration found for employee or organization.");
            }

            var academicYear = await _context.AcademicYears
                .Where(a => a.OrganizationId == employee.OrganizationId && a.IsActive)
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (academicYear == null)
            {
                academicYear = await _context.AcademicYears
                    .Where(a => a.OrganizationId == employee.OrganizationId)
                    .OrderByDescending(a => a.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (academicYear == null)
                {
                    return ApiResponseDto<AttendanceDto>.Fail("No academic year configured for organization.");
                }
            }

            var targetLocationId = request.LocationId ?? employee.ProfessionalDetails?.LocationId ?? shift.LocationId;

            if (!inDateTime.HasValue && (statusInt == (int)AttendanceStatus.Present || statusInt == (int)AttendanceStatus.Late))
            {
                inDateTime = new DateTimeOffset(request.Date.ToDateTime(shift.InTime), TimeSpan.Zero);
            }

            if (!outDateTime.HasValue && inDateTime.HasValue && (statusInt == (int)AttendanceStatus.Present || statusInt == (int)AttendanceStatus.Late))
            {
                outDateTime = new DateTimeOffset(request.Date.ToDateTime(shift.OutTime), TimeSpan.Zero);
                if (inDateTime.HasValue)
                {
                    dayTotal = Math.Round((decimal)Math.Max(0, (outDateTime.Value - inDateTime.Value).TotalHours), 2);
                }
            }

            var newAttendance = new EmployeeAttendance
            {
                OrganizationId = employee.OrganizationId,
                EmployeeId = request.EmployeeId,
                LocationId = targetLocationId,
                ShiftId = shift.Id,
                AcademicYearId = academicYear.Id,
                InTime = inDateTime,
                OutTime = outDateTime,
                DayTotal = dayTotal,
                Status = statusInt,
                Remark = !string.IsNullOrWhiteSpace(request.Remark) ? $"[Admin Marked: {request.Remark.Trim()}]" : "[Admin Marked]",
                PunchCount = outDateTime.HasValue ? 2 : 1,
                ApprovedByClientId = adminUserId
            };

            await _attendanceRepository.AddAsync(newAttendance, cancellationToken);
            _logger.LogInformation("Admin {AdminId} marked new past attendance {AttendanceId} for employee {EmployeeId} on {Date}", adminUserId, newAttendance.Id, request.EmployeeId, request.Date);

            return await MapToDtoAsync(newAttendance.Id, cancellationToken);
        }
    }

    private async Task<ApiResponseDto<AttendanceDto>> MapToDtoAsync(int attendanceId, CancellationToken cancellationToken)
    {
        var a = await _context.EmployeeAttendances
            .Include(x => x.Employee)
            .Include(x => x.Location)
            .Include(x => x.Shift)
            .FirstOrDefaultAsync(x => x.Id == attendanceId, cancellationToken);

        if (a == null) return ApiResponseDto<AttendanceDto>.Fail("Attendance record not found.");

        var dto = new AttendanceDto
        {
            Id = a.Id,
            EmployeeId = a.EmployeeId,
            EmployeeName = a.Employee != null ? $"{a.Employee.FirstName} {a.Employee.LastName}".Trim() : string.Empty,
            LocationId = a.LocationId,
            LocationName = a.Location?.Name ?? string.Empty,
            ShiftId = a.ShiftId,
            ShiftName = a.Shift?.Name ?? string.Empty,
            InTime = a.InTime,
            OutTime = a.OutTime,
            DayTotal = a.DayTotal,
            Status = ((AttendanceStatus)a.Status).ToString(),
            Remark = a.Remark
        };

        return ApiResponseDto<AttendanceDto>.Ok(dto);
    }

    // Haversine formula to calculate distance between two GPS coordinates in meters
    private static double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371e3; // Earth radius in meters
        var phi1 = lat1 * Math.PI / 180.0;
        var phi2 = lat2 * Math.PI / 180.0;
        var deltaPhi = (lat2 - lat1) * Math.PI / 180.0;
        var deltaLambda = (lon2 - lon1) * Math.PI / 180.0;

        var a = Math.Sin(deltaPhi / 2.0) * Math.Sin(deltaPhi / 2.0) +
                Math.Cos(phi1) * Math.Cos(phi2) *
                Math.Sin(deltaLambda / 2.0) * Math.Sin(deltaLambda / 2.0);

        var c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));
        return R * c;
    }
}
