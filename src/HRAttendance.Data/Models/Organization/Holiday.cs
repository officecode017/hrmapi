using HRAttendance.Data.Models.Common;

namespace HRAttendance.Data.Models.Organization;

public class Holiday : AuditableEntity
{
    public int OrganizationId { get; set; }
    public int? LocationId { get; set; }
    public int AcademicYearId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public bool IsOptional { get; set; }

    public virtual Organization Organization { get; set; } = null!;
    public virtual Location? Location { get; set; }
    public virtual AcademicYear AcademicYear { get; set; } = null!;
}
