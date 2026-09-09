using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;

namespace HRAttendance.Data.Models.Payroll;

public class StatutoryRule : AuditableEntity
{
    public int OrganizationId { get; set; }
    public StatutoryRuleType RuleType { get; set; }
    public int Version { get; set; } = 1;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string ConfigurationJson { get; set; } = "{}";
    public bool IsActive { get; set; } = true;

    public virtual Organization.Organization Organization { get; set; } = null!;
}
