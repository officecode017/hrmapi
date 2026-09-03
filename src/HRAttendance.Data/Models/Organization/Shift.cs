using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Employee;
using HRAttendance.Data.Models.Attendance;

namespace HRAttendance.Data.Models.Organization;

public class Shift : AuditableEntity
{
    public int OrganizationId { get; set; }
    public int LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public TimeOnly InTime { get; set; }
    public TimeOnly OutTime { get; set; }
    public bool IsOvernight { get; set; }
    public int GraceMinutes { get; set; }
    public int BreakMinutes { get; set; }
    public TimeOnly? BreakStartTime { get; set; }
    public TimeOnly? BreakEndTime { get; set; }

    public virtual Organization Organization { get; set; } = null!;
    public virtual Location Location { get; set; } = null!;
    public virtual ICollection<EmployeeProfessionalDetails> EmployeeProfessionalDetails { get; set; } = new List<EmployeeProfessionalDetails>();
    public virtual ICollection<EmployeeAttendance> EmployeeAttendances { get; set; } = new List<EmployeeAttendance>();
}
