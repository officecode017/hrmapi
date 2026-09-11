using HRAttendance.Business.Interfaces.Payroll;
using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Services.Payroll;

public class LOPCalculationService : ILOPCalculationService
{
    public LOPResult Calculate(
        decimal monthlyGross,
        decimal lopDays,
        int calendarDaysInMonth,
        decimal workingDaysInMonth,
        PayrollPolicy policy)
    {
        if (lopDays <= 0 || monthlyGross <= 0)
        {
            return new LOPResult(
                DailyRate: 0m,
                LOPDeduction: 0m,
                Formula: "No LOP days recorded",
                Notes: "LOP days is 0. No deduction applied."
            );
        }

        var divisor = policy.LOPBasis switch
        {
            LOPCalculationBasis.WorkingDays => workingDaysInMonth > 0 ? workingDaysInMonth : (decimal)calendarDaysInMonth,
            LOPCalculationBasis.FixedDays => policy.FixedLOPDays > 0 ? (decimal)policy.FixedLOPDays : 30m,
            _ => (decimal)calendarDaysInMonth
        };

        var dailyRate = Math.Round(monthlyGross / divisor, 4);
        var rawDeduction = lopDays >= divisor ? monthlyGross : Math.Min(monthlyGross, dailyRate * lopDays);

        var roundedDeduction = policy.RoundingRule switch
        {
            RoundingRule.NearestWholeUnit => Math.Round(rawDeduction, 0, MidpointRounding.AwayFromZero),
            RoundingRule.RoundUp => Math.Ceiling(rawDeduction),
            RoundingRule.RoundDown => Math.Floor(rawDeduction),
            _ => Math.Round(rawDeduction, 2)
        };

        roundedDeduction = Math.Min(monthlyGross, roundedDeduction);

        var formula = $"({monthlyGross} / {divisor}) * {lopDays} = {roundedDeduction}";
        var notes = $"LOP calculated using {policy.LOPBasis} basis (Divisor: {divisor}, Daily Rate: {dailyRate:F2})";

        return new LOPResult(dailyRate, roundedDeduction, formula, notes);
    }
}
