using HRAttendance.Business.Interfaces.Payroll.Statutory;

namespace HRAttendance.Business.Services.Payroll.Statutory;

public class ProfessionalTaxCalculator : IProfessionalTaxCalculator
{
    public PTCalculationResult Calculate(decimal grossSalary, int month, PTConfig config)
    {
        if (grossSalary <= 0 || config.Brackets == null || config.Brackets.Count == 0)
        {
            return new PTCalculationResult(0m, config.StateCode, "No applicable PT slab", "PT is 0.");
        }

        var bracket = config.Brackets.FirstOrDefault(b => grossSalary >= b.MinSalary && grossSalary <= b.MaxSalary);
        if (bracket == null)
        {
            return new PTCalculationResult(0m, config.StateCode, "No matching salary bracket", "PT is 0.");
        }

        var taxAmount = (month == 2 && bracket.FebruaryTax.HasValue) ? bracket.FebruaryTax.Value : bracket.MonthlyTax;
        var formula = month == 2 && bracket.FebruaryTax.HasValue
            ? $"Slab ₹{bracket.MinSalary:N0}-₹{bracket.MaxSalary:N0} (February Special Rate: ₹{taxAmount})"
            : $"Slab ₹{bracket.MinSalary:N0}-₹{bracket.MaxSalary:N0}: ₹{taxAmount}";

        var notes = $"Professional Tax for state {config.StateCode} on gross ₹{grossSalary:N0}";

        return new PTCalculationResult(taxAmount, config.StateCode, formula, notes);
    }
}
