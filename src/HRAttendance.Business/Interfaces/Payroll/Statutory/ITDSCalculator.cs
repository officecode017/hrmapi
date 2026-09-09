namespace HRAttendance.Business.Interfaces.Payroll.Statutory;

public record TDSCalculationResult(
    decimal MonthlyTDS,
    decimal ProjectedAnnualTax,
    decimal NetTaxableIncome,
    string Regime,
    string Formula,
    string Notes
);

public interface ITDSCalculator
{
    TDSCalculationResult Calculate(
        decimal monthlyTaxableGross,
        int month,
        int fiscalYearRemainingMonths,
        decimal yearToDateTaxPaid,
        decimal declaredDeductions,
        TDSConfig config
    );
}
