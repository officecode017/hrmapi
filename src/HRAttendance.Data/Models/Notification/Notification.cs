using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Employee;

namespace HRAttendance.Data.Models.Notification;

public class Notification : AuditableEntity
{
    public int OrganizationId { get; set; }
    public int EmployeeId { get; set; }
    public string NotificationHeader { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int NotificationType { get; set; }
    public DateTimeOffset Date { get; set; }
    public DateTimeOffset? ScheduleDate { get; set; }
    public bool IsScheduled { get; set; }
    public bool ReadStatus { get; set; }
    public int? OfEmployeeId { get; set; }
    public bool ForManager { get; set; }
    public string? Url { get; set; }
    public string? Metadata { get; set; }
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public string? ETag { get; set; }

    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual Models.Employee.Employee Employee { get; set; } = null!;
    public virtual Models.Employee.Employee? OfEmployee { get; set; }
}
