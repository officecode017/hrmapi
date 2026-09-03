namespace HRAttendance.Data.DTOs.Attendance;

public class CheckInRequestDto
{
    public int EmployeeId { get; set; }
    public int LocationId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? MacId { get; set; }
    public string? AppVersion { get; set; }
    public string? Remark { get; set; }
}

public class CheckOutRequestDto
{
    public int EmployeeId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? MacId { get; set; }
    public string? Remark { get; set; }
}

public class AttendanceDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int ShiftId { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public DateTimeOffset? InTime { get; set; }
    public DateTimeOffset? OutTime { get; set; }
    public decimal? DayTotal { get; set; }
    public string Status { get; set; } = string.Empty; // Present, Late, HalfDay, Absent
    public string? Remark { get; set; }
}

public class AttendanceHistoryDto
{
    public DateOnly Date { get; set; }
    public DateTimeOffset? InTime { get; set; }
    public DateTimeOffset? OutTime { get; set; }
    public decimal? TotalHours { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ShiftName { get; set; }
    public string? Remark { get; set; }
}
