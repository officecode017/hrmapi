using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;

namespace HRAttendance.Data.Models.Payroll;

public class PayrollPolicy : AuditableEntity
{
    public int OrganizationId { get; set; }
    public SalaryProrationBasis ProrationBasis { get; set; } = SalaryProrationBasis.ActualCalendarDays;
    public int FixedProrationDays { get; set; } = 30;
    public LOPCalculationBasis LOPBasis { get; set; } = LOPCalculationBasis.CalendarDays;
    public int FixedLOPDays { get; set; } = 30;
    public OvertimeBasis OTBasis { get; set; } = OvertimeBasis.BasicSalary;
    public decimal OTMultiplier { get; set; } = 1.5m;
    public decimal StandardMonthlyWorkingHours { get; set; } = 160m;
    public RoundingRule RoundingRule { get; set; } = RoundingRule.TwoDecimals;
    public bool ConsiderHolidaysInLOP { get; set; } = false;
    public bool ConsiderWeekendsInLOP { get; set; } = false;

    public virtual Organization.Organization Organization { get; set; } = null!;
}
