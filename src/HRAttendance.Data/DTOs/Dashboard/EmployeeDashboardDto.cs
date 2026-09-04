using System.Text.Json.Serialization;
using HRAttendance.Data.DTOs.Leave;

namespace HRAttendance.Data.DTOs.Dashboard;

public class EmployeeDashboardDto
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? DesignationName { get; set; }
    public string? DepartmentName { get; set; }
    public string? LocationName { get; set; }
    public string? PhotoPath { get; set; }

    // Today's Status & Terminals
    public TodayShiftSummaryDto? Shift { get; set; }
    public TodayAttendanceSummaryDto TodayAttendance { get; set; } = new();

    // Monthly KPIs
    public MonthlyAttendanceStatsDto MonthlyStats { get; set; } = new();

    // Leave Balances
    public List<LeaveBalanceDto> LeaveBalances { get; set; } = [];

    // Upcoming Public Holidays (Next 60 days)
    public List<UpcomingHolidayDto> UpcomingHolidays { get; set; } = [];

    // Recent Activity / Punches (Last 7 days)
    public List<RecentPunchActivityDto> RecentPunches { get; set; } = [];
}

public class TodayShiftSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TimeOnly InTime { get; set; }
    public TimeOnly OutTime { get; set; }
    public bool IsOvernight { get; set; }
    public int GraceMinutes { get; set; }
    public int BreakMinutes { get; set; }
    public TimeOnly? BreakStartTime { get; set; }
    public TimeOnly? BreakEndTime { get; set; }
}

public class TodayAttendanceSummaryDto
{
    public int? AttendanceId { get; set; }
    public string Status { get; set; } = "NotCheckedIn"; // "CheckedIn", "CheckedOut", "NotCheckedIn"
    public DateTimeOffset? InTime { get; set; }
    public DateTimeOffset? OutTime { get; set; }
    public double? HoursWorked { get; set; }
    public bool IsLate { get; set; }
    public string? LocationName { get; set; }
    public string? Remark { get; set; }
}

public class MonthlyAttendanceStatsDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int DaysPresent { get; set; }
    public int DaysAbsent { get; set; }
    public int LateArrivalsCount { get; set; }
    public int HalfDaysCount { get; set; }
    public int OnLeaveDaysCount { get; set; }
    public double TotalOvertimeHours { get; set; }
    public double AttendanceRatePercentage { get; set; }
}

public class UpcomingHolidayDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public bool IsOptional { get; set; }
}

public class RecentPunchActivityDto
{
    public int AttendanceId { get; set; }
    public DateOnly Date { get; set; }
    public DateTimeOffset InTime { get; set; }
    public DateTimeOffset? OutTime { get; set; }
    public double? DurationHours { get; set; }
    public string Status { get; set; } = string.Empty; // "Present", "Completed", "AutoOvertime"
    public string? LocationName { get; set; }
    public string? Remark { get; set; }
}
