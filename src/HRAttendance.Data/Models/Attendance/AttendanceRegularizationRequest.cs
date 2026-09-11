using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Employee;
using System;

namespace HRAttendance.Data.Models.Attendance;

public class AttendanceRegularizationRequest : AuditableEntity
{
    public int AttendanceId { get; set; }
    public int EmployeeId { get; set; }

    public DateTimeOffset? RequestedInTime { get; set; }
    public DateTimeOffset? RequestedOutTime { get; set; }

    public string? EmployeeRemark { get; set; }

    // Pending / Approved / Rejected
    public string Status { get; set; } = "Pending";

    public DateTimeOffset? ReviewedAt { get; set; }
    public int? ReviewedBy { get; set; }
    public string? AdminRemark { get; set; }

    public virtual EmployeeAttendance Attendance { get; set; } = null!;
    public virtual Models.Employee.Employee Employee { get; set; } = null!;
}