using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;

namespace HRAttendance.Data.Models.Payroll;

public class PayrollPeriod : AuditableEntity
{
    public int OrganizationId { get; set; }
    public int FinancialYearId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public PayrollRunType RunType { get; set; } = PayrollRunType.Regular;
    public int SequenceNumber { get; set; } = 1;
    public string Description { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public PayrollPeriodStatus Status { get; set; } = PayrollPeriodStatus.Draft;

    // Snapshot of policy and statutory rule versions at calculation time
    public string PolicySnapshotJson { get; set; } = "{}";

    public decimal TotalGrossPay { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalEmployerContributions { get; set; }
    public decimal TotalNetPay { get; set; }
    public int TotalEmployeesProcessed { get; set; }
    public int TotalExceptionsCount { get; set; }

    public DateTimeOffset? CalculatedAt { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public int? LockedBy { get; set; }
    public DateTimeOffset? LockedAt { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual FinancialYear FinancialYear { get; set; } = null!;
    public virtual ICollection<PayrollEmployee> Employees { get; set; } = new List<PayrollEmployee>();
    public virtual ICollection<PayrollException> Exceptions { get; set; } = new List<PayrollException>();
    public virtual ICollection<PayrollAdjustment> Adjustments { get; set; } = new List<PayrollAdjustment>();
    public virtual ICollection<PayrollArrear> ArrearsTargetingPeriod { get; set; } = new List<PayrollArrear>();
    public virtual ICollection<BankExportBatch> BankExports { get; set; } = new List<BankExportBatch>();
}
