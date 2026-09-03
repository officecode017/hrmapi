using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Employee;

namespace HRAttendance.Data.Models.Security;

public class EmployeeRole : AuditableEntity
{
    public int OrganizationId { get; set; }
    public int EmployeeId { get; set; }
    public int RoleId { get; set; }

    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual Models.Employee.Employee Employee { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}
