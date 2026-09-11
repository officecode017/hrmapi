namespace HRAttendance.Data.DTOs.Attendance;

public class CheckInRequestDto
{
    public int EmployeeId { get; set; }
    public int LocationId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? MacId { get; set; }
    public int? InNetworkSource { get; set; }
    public int? InPlatform { get; set; }
    public string? AppVersion { get; set; }
    public string? Remark { get; set; }
}

public class CheckOutRequestDto
{
    public int EmployeeId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? MacId { get; set; }
    public int? OutNetworkSource { get; set; }
    public int? OutPlatform { get; set; }
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
    public string Status { get; set; } = string.Empty;
    public string? Remark { get; set; }
}

public class AttendanceHistoryDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public DateTimeOffset? InTime { get; set; }
    public DateTimeOffset? OutTime { get; set; }
    public decimal? DayTotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ShiftName { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string? Remark { get; set; }
}

public class AdminMarkAttendanceRequestDto
{
    public int? AttendanceId { get; set; }
    public int EmployeeId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly? InTime { get; set; }
    public TimeOnly? OutTime { get; set; }
    public string Status { get; set; } = "Present"; // Present, Late, HalfDay, Absent, OnLeave
    public int? LocationId { get; set; }
    public int? ShiftId { get; set; }
    public string Remark { get; set; } = string.Empty;
}



public class AttendanceRegularizationRequestDto
{
    public int Id { get; set; }
    public int AttendanceId { get; set; }

    public DateTimeOffset? RequestedInTime { get; set; }

    public DateTimeOffset? RequestedOutTime { get; set; }

    public string? EmployeeRemark { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTimeOffset? ReviewedAt { get; set; }

    public int? ReviewedBy { get; set; }

    public string? AdminRemark { get; set; }
}
