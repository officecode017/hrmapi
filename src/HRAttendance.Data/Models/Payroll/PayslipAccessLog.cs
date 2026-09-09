using HRAttendance.Data.Models.Common;

namespace HRAttendance.Data.Models.Payroll;

public class PayslipAccessLog : AuditableEntity
{
    public int PayslipId { get; set; }
    public int AccessedBy { get; set; }
    public DateTimeOffset AccessedAt { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public PayslipActionType ActionType { get; set; }

    public virtual Payslip Payslip { get; set; } = null!;
}
