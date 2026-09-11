using HRAttendance.Business.Interfaces.Payroll;
using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Services.Payroll;

public class MidMonthProrationService : IMidMonthProrationService
{
    public ProrationResult CalculateSlices(
        EmploymentWindow window,
        IReadOnlyList<EmployeeSalaryStructure> structures,
        PayrollPolicy policy)
    {
        if (!window.IsEligible || structures.Count == 0)
        {
            return new ProrationResult(
                Array.Empty<PayrollSalarySlice>(),
                Array.Empty<ComponentProration>(),
                BaseMonthlyGross: 0m,
                ProratedGross: 0m
            );
        }

        var sortedStructures = structures
            .OrderBy(s => s.EffectiveFrom)
            .ToList();

        var slices = new List<PayrollSalarySlice>();
        var componentMap = new Dictionary<int, (SalaryComponent Component, decimal FullMonthly, decimal Prorated, List<string> Formulas)>();

        foreach (var structure in sortedStructures)
        {
            var sliceStart = structure.EffectiveFrom > window.StartDate ? structure.EffectiveFrom : window.StartDate;
            var structEnd = structure.EffectiveTo ?? window.EndDate;
            var sliceEnd = structEnd < window.EndDate ? structEnd : window.EndDate;

            if (sliceStart > sliceEnd) continue;

            var sliceDays = (sliceEnd.DayNumber - sliceStart.DayNumber) + 1;
            var divisor = policy.ProrationBasis switch
            {
                SalaryProrationBasis.FixedDays => policy.FixedProrationDays > 0 ? policy.FixedProrationDays : 30,
                _ => window.TotalDaysInMonth
            };

            var prorationFactor = (decimal)sliceDays / divisor;
            var proratedGross = Math.Round(structure.MonthlyGrossSalary * prorationFactor, 2);

            var slice = new PayrollSalarySlice
            {
                SalaryStructureId = structure.Id,
                SalaryStructureVersion = structure.Version,
                SliceStartDate = sliceStart,
                SliceEndDate = sliceEnd,
                TotalCalendarDaysInSlice = sliceDays,
                EligibleDaysInSlice = sliceDays,
                MonthlyGrossInSlice = structure.MonthlyGrossSalary,
                ProratedGrossInSlice = proratedGross,
                SliceNotes = $"Revision V{structure.Version}: {sliceStart:yyyy-MM-dd} to {sliceEnd:yyyy-MM-dd} ({sliceDays}/{divisor} days)",
                SalaryStructure = structure
            };
            slices.Add(slice);

            // Accumulate prorated component values
            foreach (var item in structure.Items)
            {
                var proratedItemAmt = Math.Round(item.MonthlyAmount * prorationFactor, 2);
                if (!componentMap.TryGetValue(item.SalaryComponentId, out var compEntry))
                {
                    compEntry = (item.Component, item.MonthlyAmount, 0m, new List<string>());
                }

                compEntry.Prorated += proratedItemAmt;
                compEntry.Formulas.Add($"V{structure.Version}: ({item.MonthlyAmount} * {sliceDays}/{divisor} = {proratedItemAmt})");
                componentMap[item.SalaryComponentId] = compEntry;
            }
        }

        var componentProrations = componentMap.Values.Select(c => new ComponentProration(
            SalaryComponentId: c.Component.Id,
            ComponentCode: c.Component.Code,
            ComponentName: c.Component.Name,
            Type: c.Component.Type,
            CalculationOrder: c.Component.CalculationOrder,
            CalculationType: c.Component.CalculationType,
            FullMonthlyAmount: c.FullMonthly,
            ProratedAmount: c.Prorated,
            DerivationFormula: string.Join(" + ", c.Formulas),
            DerivationNotes: $"Prorated across {slices.Count} revision slice(s)"
        ))
        .OrderBy(c => c.CalculationOrder)
        .ToList();

        var totalProratedGross = slices.Sum(s => s.ProratedGrossInSlice);
        var baseMonthlyGross = slices.Count > 0 ? slices.Last().MonthlyGrossInSlice : 0m;

        return new ProrationResult(slices, componentProrations, baseMonthlyGross, totalProratedGross);
    }
}
