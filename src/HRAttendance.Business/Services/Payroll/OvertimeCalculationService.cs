using HRAttendance.Business.Interfaces.Payroll;
using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Services.Payroll;

public class OvertimeCalculationService : IOvertimeCalculationService
{
    public OvertimeResult Calculate(
        decimal basicSalary,
        decimal grossSalary,
        decimal approvedOTHours,
        PayrollPolicy policy)
    {
        if (approvedOTHours <= 0)
        {
            return new OvertimeResult(
                HourlyRate: 0m,
                OvertimePay: 0m,
                Formula: "No approved overtime hours",
                Notes: "Approved OT hours is 0. Overtime pay is 0."
            );
        }

        var basisSalary = policy.OTBasis switch
        {
            OvertimeBasis.GrossSalary => grossSalary,
            _ => basicSalary
        };

        var standardHours = policy.StandardMonthlyWorkingHours > 0 ? policy.StandardMonthlyWorkingHours : 160m;
        var multiplier = policy.OTMultiplier > 0 ? policy.OTMultiplier : 1.5m;

        var baseHourly = basisSalary / standardHours;
        var effectiveHourlyRate = Math.Round(baseHourly * multiplier, 4);
        var rawPay = effectiveHourlyRate * approvedOTHours;

        var roundedPay = policy.RoundingRule switch
        {
            RoundingRule.NearestWholeUnit => Math.Round(rawPay, 0, MidpointRounding.AwayFromZero),
            RoundingRule.RoundUp => Math.Ceiling(rawPay),
            RoundingRule.RoundDown => Math.Floor(rawPay),
            _ => Math.Round(rawPay, 2)
        };

        var formula = $"({basisSalary} / {standardHours}) * {multiplier} * {approvedOTHours} = {roundedPay}";
        var notes = $"Overtime on {policy.OTBasis} basis at {multiplier}x rate ({approvedOTHours} hrs @ {effectiveHourlyRate:F2}/hr)";

        return new OvertimeResult(effectiveHourlyRate, roundedPay, formula, notes);
    }
}
