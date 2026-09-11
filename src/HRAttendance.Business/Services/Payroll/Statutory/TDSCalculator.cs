using HRAttendance.Business.Interfaces.Payroll.Statutory;

namespace HRAttendance.Business.Services.Payroll.Statutory;

public class TDSCalculator : ITDSCalculator
{
    public TDSCalculationResult Calculate(
        decimal monthlyTaxableGross,
        int month,
        int fiscalYearRemainingMonths,
        decimal yearToDateTaxPaid,
        decimal declaredDeductions,
        TDSConfig config)
    {
        if (monthlyTaxableGross <= 0)
        {
            return new TDSCalculationResult(0m, 0m, 0m, "New Regime", "Taxable gross is 0", "No TDS deducted.");
        }

        var remainingMonths = Math.Max(1, fiscalYearRemainingMonths);
        var projectedAnnualGross = monthlyTaxableGross * 12m;
        var netTaxableIncome = Math.Max(0m, projectedAnnualGross - config.StandardDeduction - declaredDeductions);

        // Section 87A Rebate: Under New Regime, no tax if taxable income <= 7,00,000
        if (netTaxableIncome <= config.RebateLimit)
        {
            return new TDSCalculationResult(
                MonthlyTDS: 0m,
                ProjectedAnnualTax: 0m,
                NetTaxableIncome: netTaxableIncome,
                Regime: "New Regime (Sec 115BAC)",
                Formula: $"Projected Annual: {projectedAnnualGross:N0} - Std Ded {config.StandardDeduction:N0} = {netTaxableIncome:N0} <= {config.RebateLimit:N0} (Sec 87A Rebate)",
                Notes: $"Nil tax under Section 87A rebate for taxable income up to ₹{config.RebateLimit:N0}."
            );
        }

        // New Tax Regime Slabs
        decimal baseTax = 0m;
        if (netTaxableIncome > 1500000m)
        {
            baseTax += (netTaxableIncome - 1500000m) * 0.30m;
            baseTax += 300000m * 0.20m; // 12L to 15L
            baseTax += 200000m * 0.15m; // 10L to 12L
            baseTax += 300000m * 0.10m; // 7L to 10L
            baseTax += 400000m * 0.05m; // 3L to 7L
        }
        else if (netTaxableIncome > 1200000m)
        {
            baseTax += (netTaxableIncome - 1200000m) * 0.20m;
            baseTax += 200000m * 0.15m;
            baseTax += 300000m * 0.10m;
            baseTax += 400000m * 0.05m;
        }
        else if (netTaxableIncome > 1000000m)
        {
            baseTax += (netTaxableIncome - 1000000m) * 0.15m;
            baseTax += 300000m * 0.10m;
            baseTax += 400000m * 0.05m;
        }
        else if (netTaxableIncome > 700000m)
        {
            baseTax += (netTaxableIncome - 700000m) * 0.10m;
            baseTax += 400000m * 0.05m;
        }
        else if (netTaxableIncome > 300000m)
        {
            baseTax += (netTaxableIncome - 300000m) * 0.05m;
        }

        var cess = Math.Round(baseTax * 0.04m, 2);
        var totalAnnualTax = Math.Round(baseTax + cess, 0, MidpointRounding.AwayFromZero);

        var remainingTax = Math.Max(0m, totalAnnualTax - yearToDateTaxPaid);
        var monthlyTDS = Math.Round(remainingTax / remainingMonths, 0, MidpointRounding.AwayFromZero);

        var formula = $"Taxable {netTaxableIncome:N0} -> Base Tax {baseTax:N0} + Cess {cess:N0} = {totalAnnualTax:N0}. Rem. {remainingTax:N0} / {remainingMonths} mos = {monthlyTDS:N0}";
        var notes = $"TDS amortized across {remainingMonths} remaining month(s) of financial year.";

        return new TDSCalculationResult(monthlyTDS, totalAnnualTax, netTaxableIncome, "New Regime", formula, notes);
    }
}
