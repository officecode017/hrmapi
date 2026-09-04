namespace HRAttendance.Data.DTOs.Dashboard;

public class EmployeeCalendarDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;

    public CalendarMonthSummaryDto Summary { get; set; } = new();
    public List<CalendarDayDto> Days { get; set; } = [];
}

public class CalendarMonthSummaryDto
{
    public int TotalDays { get; set; }
    public int WorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateArrivals { get; set; }
    public int LeaveDays { get; set; }
    public int Holidays { get; set; }
    public int WeeklyOffDays { get; set; }
    public double TotalWorkHours { get; set; }
    public double TotalOvertimeHours { get; set; }
}

public class CalendarDayDto
{
    public DateOnly Date { get; set; }
    public int DayOfMonth { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;

    // Status: "Present", "Absent", "OnLeave", "HalfDayLeave", "Holiday", "WeeklyOff", "Future"
    public string Status { get; set; } = "Future";
    public bool IsWorkingDay { get; set; }

    // Shift Info
    public string? ShiftName { get; set; }
    public TimeOnly? ScheduledInTime { get; set; }
    public TimeOnly? ScheduledOutTime { get; set; }

    // Actual Punch Info
    public DateTimeOffset? CheckInTime { get; set; }
    public DateTimeOffset? CheckOutTime { get; set; }
    public double? WorkDurationHours { get; set; }
    public bool IsLate { get; set; }
    public double? OvertimeHours { get; set; }

    // Context Details
    public string? HolidayName { get; set; }
    public string? LeaveTypeName { get; set; }
    public string? LeaveStatus { get; set; }
    public bool IsHalfDayLeave { get; set; }
    public string? Remarks { get; set; }
}
