using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Employee;

namespace HRAttendance.Data.Models.Leave;

public class SubLeaveApplication : AuditableEntity
{
    public int OrganizationId { get; set; }
    public int LeaveApplicationId { get; set; }
    public int EmployeeId { get; set; }
    public DateOnly LeaveDate { get; set; }
    public string Status { get; set; } = "Pending";
    public bool IsHalfDay { get; set; }
    public int? ApprovedBy { get; set; }
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public string? ETag { get; set; }

    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual LeaveApplication LeaveApplication { get; set; } = null!;
    public virtual Models.Employee.Employee Employee { get; set; } = null!;
    public virtual Models.Employee.Employee? Approver { get; set; }
}
