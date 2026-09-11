using HRAttendance.Data.Models.Common;

namespace HRAttendance.Data.Models.Payroll;

public class PayrollException : AuditableEntity
{
    public int PayrollPeriodId { get; set; }
    public int EmployeeId { get; set; }
    public ExceptionSeverity Severity { get; set; }
    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsResolved { get; set; } = false;
    public int? ResolvedBy { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }

    public virtual PayrollPeriod PayrollPeriod { get; set; } = null!;
    public virtual Models.Employee.Employee Employee { get; set; } = null!;
}
