using HRAttendance.Data.Models.Common;

namespace HRAttendance.Data.Models.Payroll;

public class PayslipItem : AuditableEntity
{
    public int PayslipId { get; set; }
    public string ComponentCode { get; set; } = string.Empty;
    public string ComponentName { get; set; } = string.Empty;
    public ComponentType ComponentType { get; set; }
    public decimal Amount { get; set; }
    public int DisplayOrder { get; set; }

    public virtual Payslip Payslip { get; set; } = null!;
}
