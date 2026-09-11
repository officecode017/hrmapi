using HRAttendance.Business.Interfaces.Payroll.Statutory;

namespace HRAttendance.Business.Services.Payroll.Statutory;

public class ESICalculator : IESICalculator
{
    public ESICalculationResult Calculate(decimal grossSalary, ESIConfig config)
    {
        if (!config.IsActive || grossSalary <= 0)
        {
            return new ESICalculationResult(0m, 0m, false, "Gross is 0 or ESI inactive", "ESI not applicable.");
        }

        if (grossSalary > config.WageThreshold)
        {
            return new ESICalculationResult(
                0m,
                0m,
                false,
                $"Gross {grossSalary} exceeds threshold {config.WageThreshold}",
                $"Gross salary ₹{grossSalary:N0} exceeds statutory threshold ₹{config.WageThreshold:N0}."
            );
        }

        var employeeESI = Math.Ceiling(grossSalary * config.EmployeeRate);
        var employerESI = Math.Ceiling(grossSalary * config.EmployerRate);

        var formula = $"Employee: {grossSalary} * {config.EmployeeRate:P2} = {employeeESI} | Employer: {grossSalary} * {config.EmployerRate:P2} = {employerESI}";
        var notes = $"ESI applicable as Gross ₹{grossSalary:N0} <= ₹{config.WageThreshold:N0}";

        return new ESICalculationResult(employeeESI, employerESI, true, formula, notes);
    }
}
