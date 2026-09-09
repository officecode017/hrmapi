namespace HRAttendance.Business.Interfaces.Payroll.Statutory;

public record ESICalculationResult(
    decimal EmployeeESI,
    decimal EmployerESI,
    bool IsApplicable,
    string Formula,
    string Notes
);

public interface IESICalculator
{
    ESICalculationResult Calculate(decimal grossSalary, ESIConfig config);
}
