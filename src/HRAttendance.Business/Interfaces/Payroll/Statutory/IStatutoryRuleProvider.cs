using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Interfaces.Payroll.Statutory;

public record PFConfig(
    decimal EmployeeRate = 0.12m,
    decimal EmployerEPSRate = 0.0833m,
    decimal EmployerEPFRate = 0.0367m,
    decimal WageCeiling = 15000m,
    decimal EPSWageCeiling = 15000m,
    bool EnforceWageCeiling = true
);

public record ESIConfig(
    decimal EmployeeRate = 0.0075m,
    decimal EmployerRate = 0.0325m,
    decimal WageThreshold = 21000m,
    bool IsActive = true
);

public record PTBracket(decimal MinSalary, decimal MaxSalary, decimal MonthlyTax, decimal? FebruaryTax = null);

public record PTConfig(
    string StateCode = "DEFAULT",
    List<PTBracket>? Brackets = null
);

public record TDSConfig(
    decimal StandardDeduction = 75000m,
    decimal RebateLimit = 700000m,
    bool DefaultToNewRegime = true
);

public interface IStatutoryRuleProvider
{
    Task<StatutoryRule?> GetActiveRuleAsync(int organizationId, StatutoryRuleType type, DateOnly date);
    Task<PFConfig> GetPFConfigAsync(int organizationId, DateOnly date);
    Task<ESIConfig> GetESIConfigAsync(int organizationId, DateOnly date);
    Task<PTConfig> GetPTConfigAsync(int organizationId, string stateCode, DateOnly date);
    Task<TDSConfig> GetTDSConfigAsync(int organizationId, DateOnly date);
}
