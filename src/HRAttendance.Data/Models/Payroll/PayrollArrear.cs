using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;

namespace HRAttendance.Data.Models.Payroll;

public class PayrollArrear : AuditableEntity
{
    public string ArrearNumber { get; set; } = string.Empty; // e.g. "ARR-2026-000045"
    public int OrganizationId { get; set; }
    public int EmployeeId { get; set; }
    public int SourcePayrollPeriodId { get; set; } // Past period underpaid/overpaid
    public int TargetPayrollPeriodId { get; set; } // Current active payout period
    public string ComponentCode { get; set; } = string.Empty;
    public decimal OriginalAmount { get; set; }
    public decimal CorrectAmount { get; set; }
    public decimal DifferenceAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ArrearStatus Status { get; set; } = ArrearStatus.Pending;

    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual Models.Employee.Employee Employee { get; set; } = null!;
    public virtual PayrollPeriod TargetPayrollPeriod { get; set; } = null!;
}
