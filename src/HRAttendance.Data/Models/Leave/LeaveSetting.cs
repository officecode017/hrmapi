using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;

namespace HRAttendance.Data.Models.Leave;

public class LeaveSetting : AuditableEntity
{
    public int OrganizationId { get; set; }
    public int LeaveTypeId { get; set; }
    public bool IsPaid { get; set; }
    public string? Gender { get; set; }
    public string? MaritalStatus { get; set; }
    public bool CanTakeHalfDay { get; set; }
    public decimal? InitialValueDuringProbation { get; set; }
    public bool ExcludeHolidays { get; set; }
    public bool ExcludeOffDays { get; set; }
    public int? IncludeHolidaysOffdaysAfterdays { get; set; }
    public int? MaximumNoConsecutiveLeaveAllowed { get; set; }
    public int? LeaveApplicationBeforeDays { get; set; }
    public decimal? CarryForwardLeaveCount { get; set; }
    public bool IsYearly { get; set; }
    public int? AccrualPeriodType { get; set; }
    public string? AccrualPeriod { get; set; }
    public decimal? MaximumAccrualYearlyLeave { get; set; }
    public decimal? Leaves { get; set; }
    public bool BackdateAllowed { get; set; }
    public decimal? MaxLeaveMonthly { get; set; }
    public int? ProbationPeriod { get; set; }
    public bool AllowedInProbation { get; set; }
    public DateTimeOffset? WebJobRunDate { get; set; }

    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual LeaveType LeaveType { get; set; } = null!;
}
