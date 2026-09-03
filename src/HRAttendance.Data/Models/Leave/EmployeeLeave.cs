using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Employee;

namespace HRAttendance.Data.Models.Leave;

public class EmployeeLeave : AuditableEntity
{
    public int OrganizationId { get; set; }
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public int AcademicYearId { get; set; }
    public decimal LeaveCredited { get; set; }
    public decimal LeaveBroughtForward { get; set; }
    public decimal LeavesTaken { get; set; }

    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual Models.Employee.Employee Employee { get; set; } = null!;
    public virtual LeaveType LeaveType { get; set; } = null!;
    public virtual AcademicYear AcademicYear { get; set; } = null!;
}
