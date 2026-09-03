using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;

namespace HRAttendance.Data.Models.Leave;

public class LeaveType : AuditableEntity
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual LeaveSetting? LeaveSetting { get; set; }
    public virtual ICollection<LeaveApplication> LeaveApplications { get; set; } = new List<LeaveApplication>();
    public virtual ICollection<EmployeeLeave> EmployeeLeaves { get; set; } = new List<EmployeeLeave>();
}
