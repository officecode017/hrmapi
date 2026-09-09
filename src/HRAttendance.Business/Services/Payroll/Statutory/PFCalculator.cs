using HRAttendance.Business.Interfaces.Payroll.Statutory;

namespace HRAttendance.Business.Services.Payroll.Statutory;

public class PFCalculator : IPFCalculator
{
    public PFCalculationResult Calculate(decimal basicSalary, PFConfig config)
    {
        if (basicSalary <= 0)
        {
            return new PFCalculationResult(0m, 0m, 0m, 0m, "Basic is 0", "No PF deducted as Basic Salary is 0.");
        }

        var eligibleWage = config.EnforceWageCeiling ? Math.Min(basicSalary, config.WageCeiling) : basicSalary;
        var employeePF = Math.Round(eligibleWage * config.EmployeeRate, 0, MidpointRounding.AwayFromZero);

        var epsWage = Math.Min(basicSalary, config.EPSWageCeiling);
        var employerEPS = Math.Round(epsWage * config.EmployerEPSRate, 0, MidpointRounding.AwayFromZero);
        var employerEPF = Math.Max(0m, employeePF - employerEPS);
        var totalEmployer = employerEPS + employerEPF;

        var formula = $"Employee PF: {eligibleWage} * {config.EmployeeRate:P0} = {employeePF} | Employer: EPS {employerEPS} + EPF {employerEPF} = {totalEmployer}";
        var notes = config.EnforceWageCeiling && basicSalary > config.WageCeiling
            ? $"Capped at statutory ceiling ₹{config.WageCeiling:N0} (Actual Basic: ₹{basicSalary:N0})"
            : $"Computed on Basic ₹{basicSalary:N0}";

        return new PFCalculationResult(employeePF, employerEPS, employerEPF, totalEmployer, formula, notes);
    }
}
