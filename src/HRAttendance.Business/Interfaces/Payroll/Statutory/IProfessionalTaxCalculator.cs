namespace HRAttendance.Business.Interfaces.Payroll.Statutory;

public record PTCalculationResult(
    decimal PTAmount,
    string StateCode,
    string Formula,
    string Notes
);

public interface IProfessionalTaxCalculator
{
    PTCalculationResult Calculate(decimal grossSalary, int month, PTConfig config);
}
