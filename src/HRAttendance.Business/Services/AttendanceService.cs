using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HRAttendance.Business.BusinessRules;
using HRAttendance.Business.Interfaces;
using HRAttendance.Data;
using HRAttendance.Data.DTOs.Attendance;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.Interfaces;
using HRAttendance.Data.Models.Attendance;

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
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
        {
            return ApiResponseDto<AttendanceDto>.Fail("Employee not found.");
        }

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

        var now = DateTimeOffset.UtcNow;
        var currentTime = TimeOnly.FromTimeSpan(now.TimeOfDay);

        // Calculate Late arrival
        var shiftGraceThreshold = shift.InTime.AddMinutes(shift.GraceMinutes);
        var status = currentTime > shiftGraceThreshold ? (int)AttendanceStatus.Late : (int)AttendanceStatus.Present;

        var attendance = new EmployeeAttendance
        {
            OrganizationId = employee.OrganizationId,
            EmployeeId = request.EmployeeId,
            LocationId = request.LocationId > 0 ? request.LocationId : (employee.ProfessionalDetails?.LocationId ?? shift.LocationId),
            ShiftId = shift.Id,
            AcademicYearId = academicYear.Id,
            InTime = now,
            InTimeLatitude = request.Latitude,
            InTimeLongitude = request.Longitude,
            CheckInMacId = request.MacId,
            AppVersion = request.AppVersion,
            Remark = request.Remark,
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

        await _attendanceRepository.UpdateAsync(attendance, cancellationToken);
        _logger.LogInformation("Employee {EmployeeId} checked out. Day total: {DayTotal} hours", request.EmployeeId, attendance.DayTotal);

        return await MapToDtoAsync(attendance.Id, cancellationToken);
    }

    public async Task<ApiResponseDto<List<AttendanceHistoryDto>>> GetHistoryAsync(int employeeId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default)
    {
        var records = await _attendanceRepository.GetHistoryAsync(employeeId, fromDate, toDate, cancellationToken);

        var dtos = records.Select(a => new AttendanceHistoryDto
        {
            Date = a.InTime.HasValue ? DateOnly.FromDateTime(a.InTime.Value.DateTime) : DateOnly.MinValue,
            InTime = a.InTime,
            OutTime = a.OutTime,
            TotalHours = a.DayTotal,
            Status = ((AttendanceStatus)a.Status).ToString(),
            ShiftName = a.Shift?.Name,
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
            return ApiResponseDto<AttendanceDto?>.Ok(null, "No attendance record for today.");
        }

        var dtoResult = await MapToDtoAsync(attendance.Id, cancellationToken);
        return ApiResponseDto<AttendanceDto?>.Ok(dtoResult.Data);
    }

    private async Task<ApiResponseDto<AttendanceDto>> MapToDtoAsync(int attendanceId, CancellationToken cancellationToken)
    {
        var attendance = await _context.EmployeeAttendances
            .Include(a => a.Employee)
            .Include(a => a.Location)
            .Include(a => a.Shift)
            .FirstOrDefaultAsync(a => a.Id == attendanceId, cancellationToken);

        if (attendance == null)
        {
            return ApiResponseDto<AttendanceDto>.Fail("Attendance not found.");
        }

        var dto = new AttendanceDto
        {
            Id = attendance.Id,
            EmployeeId = attendance.EmployeeId,
            EmployeeName = $"{attendance.Employee.FirstName} {attendance.Employee.LastName}".Trim(),
            LocationId = attendance.LocationId,
            LocationName = attendance.Location.Name,
            ShiftId = attendance.ShiftId,
            ShiftName = attendance.Shift.Name,
            InTime = attendance.InTime,
            OutTime = attendance.OutTime,
            DayTotal = attendance.DayTotal,
            Status = ((AttendanceStatus)attendance.Status).ToString(),
            Remark = attendance.Remark
        };

        return ApiResponseDto<AttendanceDto>.Ok(dto);
    }
}
