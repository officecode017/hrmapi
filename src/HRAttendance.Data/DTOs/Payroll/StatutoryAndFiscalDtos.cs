using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Data.DTOs.Payroll;

public class CreateStatutoryRuleDto
{
    public StatutoryRuleType RuleType { get; set; }
    public int Version { get; set; } = 1;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string ConfigurationJson { get; set; } = "{}";
}

public class StatutoryRuleDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public StatutoryRuleType RuleType { get; set; }
    public int Version { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string ConfigurationJson { get; set; } = "{}";
    public bool IsActive { get; set; }
}

public class CreateFinancialYearDto
{
    public string YearCode { get; set; } = string.Empty; // e.g. "2026-27"
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; }
}

public class FinancialYearDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string YearCode { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; }
    public List<PayrollPeriodDto> Periods { get; set; } = new();
}
