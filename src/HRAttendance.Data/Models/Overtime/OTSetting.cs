using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;

namespace HRAttendance.Data.Models.Overtime;

public class OTSetting : AuditableEntity
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsOverTimeEnabled { get; set; }
    public int OTStartAfterMinutes { get; set; }
    public decimal Multiplier { get; set; }
    public decimal? MaxOTHoursPerDay { get; set; }
    public decimal? MaxOTHoursPerWeek { get; set; }
    public decimal? MaxOTHoursPerMonth { get; set; }
    public bool IsActive { get; set; }

    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual ICollection<OTEntry> OTEntries { get; set; } = new List<OTEntry>();
}
