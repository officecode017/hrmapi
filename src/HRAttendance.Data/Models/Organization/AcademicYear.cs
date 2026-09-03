using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Attendance;
using HRAttendance.Data.Models.Leave;

namespace HRAttendance.Data.Models.Organization;

public class AcademicYear : AuditableEntity
{
    public int OrganizationId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; }

    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<Holiday> Holidays { get; set; } = new List<Holiday>();
    public virtual ICollection<OffDay> OffDays { get; set; } = new List<OffDay>();
    public virtual ICollection<EmployeeAttendance> EmployeeAttendances { get; set; } = new List<EmployeeAttendance>();
    public virtual ICollection<EmployeeLeave> EmployeeLeaves { get; set; } = new List<EmployeeLeave>();
}
