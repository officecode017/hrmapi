using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;

namespace HRAttendance.Data.Models.Payroll;

public class BankExportBatch : AuditableEntity
{
    public int OrganizationId { get; set; }
    public int PayrollPeriodId { get; set; }
    public string BatchNumber { get; set; } = string.Empty; // e.g. "EXP-202609-001"
    public string Format { get; set; } = "CSV_NEFT";       // CSV_NEFT, EXCEL_HDFC, EXCEL_ICICI
    public int TotalRecords { get; set; }
    public decimal TotalAmount { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public DateTimeOffset ExportedAt { get; set; }
    public int ExportedBy { get; set; }

    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual PayrollPeriod PayrollPeriod { get; set; } = null!;
}
