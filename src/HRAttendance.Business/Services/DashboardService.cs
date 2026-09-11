using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HRAttendance.Business.Interfaces;
using HRAttendance.Data;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Dashboard;
using HRAttendance.Data.DTOs.Leave;
using HRAttendance.Data.Models.Organization;

namespace HRAttendance.Business.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(ApplicationDbContext context, ILogger<DashboardService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponseDto<EmployeeDashboardDto>> GetEmployeeDashboardAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await _context.Employees
            .Include(e => e.ProfessionalDetails)
                .ThenInclude(p => p!.Department)
            .Include(e => e.ProfessionalDetails)
                .ThenInclude(p => p!.Designation)
            .Include(e => e.ProfessionalDetails)
                .ThenInclude(p => p!.Location)
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);

        if (employee == null)
        {
            return ApiResponseDto<EmployeeDashboardDto>.Fail("Employee not found.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startOfDay = new DateTimeOffset(today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var endOfDay = new DateTimeOffset(today.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        // 1. Shift details
        TodayShiftSummaryDto? shiftDto = null;
        Shift? shift = null;
        var shiftId = employee.ProfessionalDetails?.ShiftId;
        if (shiftId.HasValue)
        {
            shift = await _context.Shifts.FirstOrDefaultAsync(s => s.Id == shiftId.Value, cancellationToken);
            if (shift != null)
            {
                shiftDto = new TodayShiftSummaryDto
                {
                    Id = shift.Id,
                    Name = shift.Name,
                    InTime = shift.InTime,
                    OutTime = shift.OutTime,
                    IsOvernight = shift.IsOvernight,
                    GraceMinutes = shift.GraceMinutes,
                    BreakMinutes = shift.BreakMinutes,
                    BreakStartTime = shift.BreakStartTime,
                    BreakEndTime = shift.BreakEndTime
                };
            }
        }

        // 2. Today's attendance punch
        var todayAttendance = await _context.EmployeeAttendances
            .Include(a => a.Location)
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.InTime >= startOfDay && a.InTime <= endOfDay, cancellationToken);

        var todayPunchDto = new TodayAttendanceSummaryDto();
        if (todayAttendance != null && todayAttendance.InTime.HasValue)
        {
            todayPunchDto.AttendanceId = todayAttendance.Id;
            todayPunchDto.InTime = todayAttendance.InTime;
            todayPunchDto.OutTime = todayAttendance.OutTime;
            todayPunchDto.LocationName = todayAttendance.Location?.Name ?? employee.ProfessionalDetails?.Location?.Name;
            todayPunchDto.Remark = todayAttendance.Remark;

            if (todayAttendance.OutTime.HasValue)
            {
                todayPunchDto.Status = "CheckedOut";
                todayPunchDto.HoursWorked = Math.Round((todayAttendance.OutTime.Value - todayAttendance.InTime.Value).TotalHours, 2);
            }
            else
            {
                todayPunchDto.Status = "CheckedIn";
                todayPunchDto.HoursWorked = Math.Round((DateTimeOffset.UtcNow - todayAttendance.InTime.Value).TotalHours, 2);
            }

            if (shift != null)
            {
                var allowedInTime = shift.InTime.ToTimeSpan().Add(TimeSpan.FromMinutes(shift.GraceMinutes));
                todayPunchDto.IsLate = todayAttendance.InTime.Value.TimeOfDay > allowedInTime;
            }
        }
        else
        {
            todayPunchDto.Status = "NotCheckedIn";
        }

        // 3. Current Month KPIs
        var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
        var monthStart = new DateTimeOffset(new DateTime(today.Year, today.Month, 1), TimeSpan.Zero);
        var monthEnd = new DateTimeOffset(new DateTime(today.Year, today.Month, daysInMonth, 23, 59, 59), TimeSpan.Zero);

        var monthAttendances = await _context.EmployeeAttendances
            .Where(a => a.EmployeeId == employeeId && a.InTime >= monthStart && a.InTime <= monthEnd)
            .ToListAsync(cancellationToken);

        var monthLeaves = await _context.SubLeaveApplications
            .Where(s => s.EmployeeId == employeeId &&
                        s.LeaveDate.Year == today.Year &&
                        s.LeaveDate.Month == today.Month &&
                        s.Status == "Approved")
            .ToListAsync(cancellationToken);

        var distinctPresentDays = monthAttendances
            .Where(a => a.InTime.HasValue)
            .Select(a => DateOnly.FromDateTime(a.InTime!.Value.DateTime))
            .Distinct()
            .Count();

        var lateCount = 0;
        foreach (var att in monthAttendances)
        {
            if (shift != null && att.InTime.HasValue)
            {
                var allowedTime = shift.InTime.ToTimeSpan().Add(TimeSpan.FromMinutes(shift.GraceMinutes));
                if (att.InTime.Value.TimeOfDay > allowedTime) lateCount++;
            }
        }

        var totalOvertimeHours = await _context.OTEntries
            .Where(ot => ot.EmployeeId == employeeId &&
                         ot.OTDate.Year == today.Year &&
                         ot.OTDate.Month == today.Month)
            .SumAsync(ot => (double)ot.OTHours, cancellationToken);

        // Days elapsed so far this month for attendance rate
        var elapsedDays = Math.Max(1, today.Day);
        var attendanceRate = Math.Round((double)distinctPresentDays / elapsedDays * 100, 1);

        var monthlyStats = new MonthlyAttendanceStatsDto
        {
            Year = today.Year,
            Month = today.Month,
            DaysPresent = distinctPresentDays,
            DaysAbsent = Math.Max(0, elapsedDays - distinctPresentDays - monthLeaves.Count),
            LateArrivalsCount = lateCount,
            HalfDaysCount = monthLeaves.Count(l => l.IsHalfDay),
            OnLeaveDaysCount = monthLeaves.Count,
            TotalOvertimeHours = Math.Round(totalOvertimeHours, 2),
            AttendanceRatePercentage = attendanceRate
        };

        // 4. Leave Balances for Active Academic Year
        var academicYear = await _context.AcademicYears
            .Where(a => a.OrganizationId == employee.OrganizationId && a.IsActive)
            .OrderByDescending(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var balanceDtos = new List<LeaveBalanceDto>();
        if (academicYear != null)
        {
            var balances = await _context.EmployeeLeaves
                .Include(b => b.LeaveType)
                .Where(b => b.EmployeeId == employeeId && b.AcademicYearId == academicYear.Id)
                .ToListAsync(cancellationToken);

            balanceDtos = balances.Select(b => new LeaveBalanceDto
            {
                LeaveTypeId = b.LeaveTypeId,
                LeaveTypeName = b.LeaveType?.Name ?? "Unknown",
                Credited = b.LeaveCredited,
                BroughtForward = b.LeaveBroughtForward,
                Taken = b.LeavesTaken
            }).ToList();
        }

        // 5. Upcoming Holidays (Next 60 days)
        var maxHolidayDate = today.AddDays(60);
        var locationId = employee.ProfessionalDetails?.LocationId;
        var upcomingHolidays = await _context.Holidays
            .Where(h => h.OrganizationId == employee.OrganizationId &&
                        h.Date >= today &&
                        h.Date <= maxHolidayDate &&
                        (h.LocationId == null || h.LocationId == locationId))
            .OrderBy(h => h.Date)
            .Take(5)
            .Select(h => new UpcomingHolidayDto
            {
                Id = h.Id,
                Name = h.Name,
                Date = h.Date,
                DayOfWeek = h.Date.DayOfWeek.ToString(),
                IsOptional = h.IsOptional
            })
            .ToListAsync(cancellationToken);

        // 6. Recent Punch Activity (Last 7 days)
        var recentSevenDaysStart = startOfDay.AddDays(-7);
        var recentAttendances = await _context.EmployeeAttendances
            .Include(a => a.Location)
            .Where(a => a.EmployeeId == employeeId && a.InTime >= recentSevenDaysStart)
            .OrderByDescending(a => a.InTime)
            .Take(7)
            .ToListAsync(cancellationToken);

        var recentPunches = recentAttendances
            .Where(a => a.InTime.HasValue)
            .Select(a => new RecentPunchActivityDto
            {
                AttendanceId = a.Id,
                Date = DateOnly.FromDateTime(a.InTime!.Value.DateTime),
                InTime = a.InTime.Value,
                OutTime = a.OutTime,
                DurationHours = a.OutTime.HasValue ? Math.Round((a.OutTime.Value - a.InTime.Value).TotalHours, 2) : null,
                Status = a.OutTime.HasValue ? "Completed" : "Active",
                LocationName = a.Location?.Name ?? employee.ProfessionalDetails?.Location?.Name,
                Remark = a.Remark
            }).ToList();

        var result = new EmployeeDashboardDto
        {
            EmployeeId = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            FullName = $"{employee.FirstName} {employee.LastName}".Trim(),
            DesignationName = employee.ProfessionalDetails?.Designation?.Name,
            DepartmentName = employee.ProfessionalDetails?.Department?.Name,
            LocationName = employee.ProfessionalDetails?.Location?.Name,
            PhotoPath = employee.PhotoPath,
            Shift = shiftDto,
            TodayAttendance = todayPunchDto,
            MonthlyStats = monthlyStats,
            LeaveBalances = balanceDtos,
            UpcomingHolidays = upcomingHolidays,
            RecentPunches = recentPunches
        };

        return ApiResponseDto<EmployeeDashboardDto>.Ok(result);
    }

    public async Task<ApiResponseDto<EmployeeCalendarDto>> GetEmployeeCalendarAsync(int employeeId, int year, int month, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        if (year <= 0) year = now.Year;
        if (month < 1 || month > 12) month = now.Month;

        var employee = await _context.Employees
            .Include(e => e.ProfessionalDetails)
            .Include(e => e.EmployeeRoles)
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);

        if (employee == null)
        {
            return ApiResponseDto<EmployeeCalendarDto>.Fail("Employee not found.");
        }

        Shift? shift = null;
        var shiftId = employee.ProfessionalDetails?.ShiftId;
        if (shiftId.HasValue)
        {
            shift = await _context.Shifts.FirstOrDefaultAsync(s => s.Id == shiftId.Value, cancellationToken);
        }

        var daysInMonth = DateTime.DaysInMonth(year, month);
        var startDate = new DateOnly(year, month, 1);
        var endDate = new DateOnly(year, month, daysInMonth);
        var startDateTime = new DateTimeOffset(startDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var endDateTime = new DateTimeOffset(endDate.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
        var today = DateOnly.FromDateTime(now);
        var locationId = employee.ProfessionalDetails?.LocationId;

        // Query Punches
        var attendances = await _context.EmployeeAttendances
            .Where(a => a.EmployeeId == employeeId && a.InTime >= startDateTime && a.InTime <= endDateTime)
            .ToListAsync(cancellationToken);

        // Query Approved & Pending Leaves
        var subLeaves = await _context.SubLeaveApplications
            .Include(s => s.LeaveApplication)
            .ThenInclude(l => l.LeaveType)
            .Where(s => s.EmployeeId == employeeId &&
                        s.LeaveDate >= startDate &&
                        s.LeaveDate <= endDate &&
                        s.Status != "Rejected" &&
                        s.Status != "Cancelled")
            .ToListAsync(cancellationToken);

        // Query Public Holidays
        var holidays = await _context.Holidays
            .Where(h => h.OrganizationId == employee.OrganizationId &&
                        h.Date >= startDate &&
                        h.Date <= endDate &&
                        (h.LocationId == null || h.LocationId == locationId))
            .ToListAsync(cancellationToken);

        // Query Weekly Off-Days for Employee
        var roleIds = employee.EmployeeRoles.Select(r => r.RoleId).ToList();
        var offDayRules = await _context.OffDays
            .Where(o => o.OrganizationId == employee.OrganizationId &&
                        (locationId == null || o.LocationId == 0 || o.LocationId == locationId) &&
                        (roleIds.Count == 0 || o.RoleId == 0 || roleIds.Contains(o.RoleId)))
            .ToListAsync(cancellationToken);

        // Query Overtime entries for the month
        var otEntries = await _context.OTEntries
            .Where(ot => ot.EmployeeId == employeeId &&
                         ot.OTDate >= startDate &&
                         ot.OTDate <= endDate)
            .ToListAsync(cancellationToken);

        var dayList = new List<CalendarDayDto>();

        for (int d = 1; d <= daysInMonth; d++)
        {
            var date = new DateOnly(year, month, d);
            var dayOfWeek = date.DayOfWeek;

            var dayPunch = attendances.FirstOrDefault(a => a.InTime.HasValue && DateOnly.FromDateTime(a.InTime.Value.DateTime) == date);
            var dayLeave = subLeaves.FirstOrDefault(s => s.LeaveDate == date);
            var dayHoliday = holidays.FirstOrDefault(h => h.Date == date);
            var isOffDay = CheckIfOffDay(date, offDayRules);
            var dayOt = otEntries.FirstOrDefault(ot => ot.OTDate == date);

            var calendarDay = new CalendarDayDto
            {
                Date = date,
                DayOfMonth = d,
                DayOfWeek = dayOfWeek.ToString(),
                ShiftName = shift?.Name,
                ScheduledInTime = shift?.InTime,
                ScheduledOutTime = shift?.OutTime,
                IsWorkingDay = !isOffDay && dayHoliday == null
            };

            // Resolution hierarchy
            if (dayPunch != null && dayPunch.InTime.HasValue)
            {
                calendarDay.AttendanceId = dayPunch.Id;
                calendarDay.Status = "Present";
                calendarDay.CheckInTime = dayPunch.InTime;
                calendarDay.CheckOutTime = dayPunch.OutTime;
                if (dayPunch.OutTime.HasValue)
                {
                    calendarDay.WorkDurationHours = Math.Round((dayPunch.OutTime.Value - dayPunch.InTime.Value).TotalHours, 2);
                }

                if (shift != null)
                {
                    var allowedIn = shift.InTime.ToTimeSpan().Add(TimeSpan.FromMinutes(shift.GraceMinutes));
                    calendarDay.IsLate = dayPunch.InTime.Value.TimeOfDay > allowedIn;
                }

                if (dayOt != null)
                {
                    calendarDay.OvertimeHours = (double)dayOt.OTHours;
                }

                if (dayHoliday != null)
                {
                    calendarDay.Remarks = $"Worked on Holiday: {dayHoliday.Name}";
                    calendarDay.HolidayName = dayHoliday.Name;
                }
                else if (isOffDay)
                {
                    calendarDay.Remarks = "Worked on Weekly Off";
                }
                else if (!dayPunch.OutTime.HasValue)
                {
                    calendarDay.Remarks = "Missing Punch Out";
                }
            }
            else if (dayLeave != null)
            {
                calendarDay.Status = dayLeave.IsHalfDay ? "HalfDayLeave" : "OnLeave";
                calendarDay.IsHalfDayLeave = dayLeave.IsHalfDay;
                calendarDay.LeaveTypeName = dayLeave.LeaveApplication?.LeaveType?.Name ?? "Leave";
                calendarDay.LeaveStatus = dayLeave.Status;
                calendarDay.Remarks = dayLeave.LeaveApplication?.Reason;
            }
            else if (dayHoliday != null)
            {
                calendarDay.Status = "Holiday";
                calendarDay.HolidayName = dayHoliday.Name;
                calendarDay.Remarks = dayHoliday.IsOptional ? "Optional Holiday" : "Public Holiday";
            }
            else if (isOffDay)
            {
                calendarDay.Status = "WeeklyOff";
                calendarDay.Remarks = "Scheduled Weekly Off";
            }
            else if (date < today)
            {
                calendarDay.Status = "Absent";
                calendarDay.Remarks = "Unexcused Absence";
            }
            else
            {
                calendarDay.Status = "Future";
            }

            dayList.Add(calendarDay);
        }

        // Summary Calculations
        var summary = new CalendarMonthSummaryDto
        {
            TotalDays = daysInMonth,
            WorkingDays = dayList.Count(d => d.IsWorkingDay),
            PresentDays = dayList.Count(d => d.Status == "Present"),
            AbsentDays = dayList.Count(d => d.Status == "Absent"),
            LateArrivals = dayList.Count(d => d.IsLate),
            LeaveDays = dayList.Count(d => d.Status == "OnLeave" || d.Status == "HalfDayLeave"),
            Holidays = dayList.Count(d => d.Status == "Holiday"),
            WeeklyOffDays = dayList.Count(d => d.Status == "WeeklyOff"),
            TotalWorkHours = Math.Round(dayList.Sum(d => d.WorkDurationHours ?? 0), 2),
            TotalOvertimeHours = Math.Round(dayList.Sum(d => d.OvertimeHours ?? 0), 2)
        };

        var calendarDto = new EmployeeCalendarDto
        {
            EmployeeId = employee.Id,
            EmployeeName = $"{employee.FirstName} {employee.LastName}".Trim(),
            Year = year,
            Month = month,
            MonthName = new DateTime(year, month, 1).ToString("MMMM"),
            Summary = summary,
            Days = dayList
        };

        return ApiResponseDto<EmployeeCalendarDto>.Ok(calendarDto);
    }

    private static bool CheckIfOffDay(DateOnly date, List<OffDay> offDayRules)
    {
        var dayName = date.DayOfWeek.ToString();
        var weekNumber = ((date.Day - 1) / 7) + 1; // 1, 2, 3, 4, 5

        foreach (var rule in offDayRules)
        {
            if (rule.OffDayName.Equals(dayName, StringComparison.OrdinalIgnoreCase))
            {
                bool isOff = weekNumber switch
                {
                    1 => rule.Week1,
                    2 => rule.Week2,
                    3 => rule.Week3,
                    4 => rule.Week4,
                    5 => rule.Week5,
                    _ => rule.Week6
                };

                if (isOff) return true;
            }
        }

        return false;
    }
}
