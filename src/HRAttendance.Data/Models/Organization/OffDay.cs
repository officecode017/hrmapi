using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Security;

namespace HRAttendance.Data.Models.Organization;

public class OffDay : AuditableEntity
{
    public int OrganizationId { get; set; }
    public int AcademicYearId { get; set; }
    public int LocationId { get; set; }
    public int RoleId { get; set; }
    public string OffDayName { get; set; } = string.Empty; // ERD: OffDay : varchar
    public int WorkDayType { get; set; }
    public bool Week1 { get; set; }
    public bool Week2 { get; set; }
    public bool Week3 { get; set; }
    public bool Week4 { get; set; }
    public bool Week5 { get; set; }
    public bool Week6 { get; set; }

    public virtual Organization Organization { get; set; } = null!;
    public virtual AcademicYear AcademicYear { get; set; } = null!;
    public virtual Location Location { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}
