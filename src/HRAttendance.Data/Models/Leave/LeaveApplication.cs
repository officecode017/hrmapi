using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Employee;

namespace HRAttendance.Data.Models.Leave;

public class LeaveApplication : AuditableEntity
{
    public int OrganizationId { get; set; }
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public decimal NoOfLeave { get; set; }
    public DateTimeOffset LeaveFrom { get; set; }
    public DateTimeOffset LeaveTo { get; set; }
    public DateTimeOffset LeaveApplicationDate { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Reason { get; set; }
    public int? ApprovedBy { get; set; }
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public string? ETag { get; set; }

    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual Models.Employee.Employee Employee { get; set; } = null!;
    public virtual LeaveType LeaveType { get; set; } = null!;
    public virtual Models.Employee.Employee? Approver { get; set; }
    public virtual ICollection<SubLeaveApplication> SubLeaveApplications { get; set; } = new List<SubLeaveApplication>();
}
