using HRAttendance.Data.Models.Common;

namespace HRAttendance.Data.Models.Payroll;

public class Payslip : AuditableEntity
{
    public int PayrollEmployeeId { get; set; }
    public int EmployeeId { get; set; }
    public string PayslipNumber { get; set; } = string.Empty; // e.g. "PAY-202609-000142"
    public PayslipStatus Status { get; set; } = PayslipStatus.Draft;
    public DateTimeOffset GeneratedAt { get; set; }
    public string? StoragePath { get; set; }
    public string? DocumentHash { get; set; }

    public virtual PayrollEmployee PayrollEmployee { get; set; } = null!;
    public virtual Models.Employee.Employee Employee { get; set; } = null!;
    public virtual ICollection<PayslipItem> Items { get; set; } = new List<PayslipItem>();
    public virtual ICollection<PayslipAccessLog> AccessLogs { get; set; } = new List<PayslipAccessLog>();
}
