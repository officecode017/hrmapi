using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;

namespace HRAttendance.Data.Models.Payroll;

public class FinancialYear : AuditableEntity
{
    public int OrganizationId { get; set; }
    public string YearCode { get; set; } = string.Empty; // e.g. "2026-27"
    public DateOnly StartDate { get; set; }              // 2026-04-01
    public DateOnly EndDate { get; set; }                // 2027-03-31
    public bool IsActive { get; set; }

    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual ICollection<PayrollPeriod> PayrollPeriods { get; set; } = new List<PayrollPeriod>();
}
