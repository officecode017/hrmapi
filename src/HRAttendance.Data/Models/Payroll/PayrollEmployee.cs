using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;

namespace HRAttendance.Data.Models.Payroll;

public class PayrollEmployee : AuditableEntity
{
    public int PayrollPeriodId { get; set; }
    public int EmployeeId { get; set; }
    public int OrganizationId { get; set; }
    public int CalculationVersion { get; set; } = 1;

    // Employment & Attendance Snapshot
    public int EligibleEmploymentDays { get; set; }
    public int CalendarDaysInMonth { get; set; }
    public decimal WorkingDays { get; set; }
    public decimal PresentDays { get; set; }
    public decimal PaidLeaveDays { get; set; }
    public decimal HalfDays { get; set; }
    public decimal LOPDays { get; set; }
    public decimal ApprovedOvertimeHours { get; set; }

    // Financial Ledger Totals
    public decimal BaseMonthlyGross { get; set; }
    public decimal ProratedGross { get; set; }
    public decimal LOPDeduction { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal AdjustmentsTotal { get; set; }
    public decimal ArrearsTotal { get; set; }
    public decimal GrossEarnings { get; set; }
    public decimal StatutoryDeductions { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal EmployerContributions { get; set; }
    public decimal NetPay { get; set; }

    public PayrollEmployeeStatus Status { get; set; } = PayrollEmployeeStatus.Pending;
    public DateTimeOffset CalculatedAt { get; set; }

    public virtual PayrollPeriod Period { get; set; } = null!;
    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual Models.Employee.Employee Employee { get; set; } = null!;
    public virtual ICollection<PayrollSalarySlice> Slices { get; set; } = new List<PayrollSalarySlice>();
    public virtual ICollection<PayrollItem> Items { get; set; } = new List<PayrollItem>();
    public virtual ICollection<PayrollPayment> Payments { get; set; } = new List<PayrollPayment>();
    public virtual Payslip? Payslip { get; set; }
}
