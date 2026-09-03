using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Employee;

namespace HRAttendance.Data.Models.Overtime;

public class OTEntry : AuditableEntity
{
    public int OrganizationId { get; set; }
    public int EmployeeId { get; set; }
    public int OTSettingId { get; set; }
    public DateOnly OTDate { get; set; }
    public DateTimeOffset? ShiftEndTime { get; set; }
    public DateTimeOffset? ActualOutTime { get; set; }
    public decimal OTHours { get; set; }
    public decimal MultiplierApplied { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal OTAmount { get; set; }

    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual Models.Employee.Employee Employee { get; set; } = null!;
    public virtual OTSetting OTSetting { get; set; } = null!;
}
