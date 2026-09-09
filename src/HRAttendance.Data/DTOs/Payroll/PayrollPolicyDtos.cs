using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Data.DTOs.Payroll;

public class CreateOrUpdatePayrollPolicyDto
{
    public SalaryProrationBasis ProrationBasis { get; set; } = SalaryProrationBasis.ActualCalendarDays;
    public int FixedProrationDays { get; set; } = 30;
    public LOPCalculationBasis LOPBasis { get; set; } = LOPCalculationBasis.CalendarDays;
    public int FixedLOPDays { get; set; } = 30;
    public OvertimeBasis OTBasis { get; set; } = OvertimeBasis.BasicSalary;
    public decimal OTMultiplier { get; set; } = 1.5m;
    public decimal StandardMonthlyWorkingHours { get; set; } = 160m;
    public RoundingRule RoundingRule { get; set; } = RoundingRule.NearestWholeUnit;
    public bool ConsiderHolidaysInLOP { get; set; } = false;
    public bool ConsiderWeekendsInLOP { get; set; } = false;
}

public class PayrollPolicyDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public SalaryProrationBasis ProrationBasis { get; set; }
    public int FixedProrationDays { get; set; }
    public LOPCalculationBasis LOPBasis { get; set; }
    public int FixedLOPDays { get; set; }
    public OvertimeBasis OTBasis { get; set; }
    public decimal OTMultiplier { get; set; }
    public decimal StandardMonthlyWorkingHours { get; set; }
    public RoundingRule RoundingRule { get; set; }
    public bool ConsiderHolidaysInLOP { get; set; }
    public bool ConsiderWeekendsInLOP { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; }
    public int? ModifiedBy { get; set; }
}

public class PayrollPolicyHistoryDto
{
    public int Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public int? ModifiedBy { get; set; }
    public string ModifiedByName { get; set; } = string.Empty;
    public string ChangeSummary { get; set; } = string.Empty;
}
