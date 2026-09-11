namespace HRAttendance.Business.Interfaces.Payroll.Statutory;

public record PFCalculationResult(
    decimal EmployeePF,
    decimal EmployerEPS,
    decimal EmployerEPF,
    decimal TotalEmployerPF,
    string Formula,
    string Notes
);

public interface IPFCalculator
{
    PFCalculationResult Calculate(decimal basicSalary, PFConfig config);
}
