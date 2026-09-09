using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Interfaces.Payroll.Statutory;

public record StatutoryDeductionsResult(
    PFCalculationResult? PF,
    ESICalculationResult? ESI,
    PTCalculationResult? PT,
    TDSCalculationResult? TDS,
    decimal TotalEmployeeDeductions,
    decimal TotalEmployerContributions,
    IReadOnlyList<PayrollItem> GeneratedItems
);

public interface IStatutoryCalculationService
{
    Task<StatutoryDeductionsResult> CalculateStatutoryAsync(
        int organizationId,
        int employeeId,
        string stateCode,
        decimal basicSalary,
        decimal grossSalary,
        int year,
        int month,
        DateOnly calculationDate
    );
}
