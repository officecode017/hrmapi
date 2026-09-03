using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Employee;

namespace HRAttendance.Data.Models.Attendance;

public class EmployeeAttendance : AuditableEntity
{
    public int OrganizationId { get; set; }
    public int EmployeeId { get; set; }
    public int LocationId { get; set; }
    public int ShiftId { get; set; }
    public int AcademicYearId { get; set; }
    public DateTimeOffset? InTime { get; set; }
    public DateTimeOffset? OutTime { get; set; }
    public decimal? DayTotal { get; set; }
    public decimal? InTimeLatitude { get; set; }
    public decimal? InTimeLongitude { get; set; }
    public decimal? OutTimeLatitude { get; set; }
    public decimal? OutTimeLongitude { get; set; }
    public string? Remark { get; set; }
    public int? ApprovedByClientId { get; set; }
    public int Status { get; set; } // 1: Present, 2: Late, 3: HalfDay, 4: Absent, 5: OnLeave, 6: Holiday, 7: Weekend
    public string? CheckInMacId { get; set; }
    public string? CheckOutMacId { get; set; }
    public int? InPlatform { get; set; }
    public int? OutPlatform { get; set; }
    public int? InNetworkSource { get; set; }
    public int? OutNetworkSource { get; set; }
    public string? AppVersion { get; set; }
    public int? PunchCount { get; set; }
    public bool IsProcessedForPayroll { get; set; }
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public string? ETag { get; set; }

    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual Models.Employee.Employee Employee { get; set; } = null!;
    public virtual Location Location { get; set; } = null!;
    public virtual Shift Shift { get; set; } = null!;
    public virtual AcademicYear AcademicYear { get; set; } = null!;
}
