using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;

namespace HRAttendance.Data.Models.Payroll;

public class PayrollAdjustment : AuditableEntity
{
    public string AdjustmentNumber { get; set; } = string.Empty; // e.g. "ADJ-2026-000123"
    public int OrganizationId { get; set; }
    public int EmployeeId { get; set; }
    public int PayrollPeriodId { get; set; }
    public AdjustmentType Type { get; set; }
    public AdjustmentDirection Direction { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public AdjustmentStatus Status { get; set; } = AdjustmentStatus.Pending;

    public int? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public int? RejectedBy { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }

    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual Models.Employee.Employee Employee { get; set; } = null!;
    public virtual PayrollPeriod PayrollPeriod { get; set; } = null!;
}
