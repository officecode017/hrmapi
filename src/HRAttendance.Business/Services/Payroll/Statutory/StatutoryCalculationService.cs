using HRAttendance.Business.Interfaces.Payroll.Statutory;
using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Services.Payroll.Statutory;

public class StatutoryCalculationService : IStatutoryCalculationService
{
    private readonly IStatutoryRuleProvider _ruleProvider;
    private readonly IPFCalculator _pfCalculator;
    private readonly IESICalculator _esiCalculator;
    private readonly IProfessionalTaxCalculator _ptCalculator;
    private readonly ITDSCalculator _tdsCalculator;

    public StatutoryCalculationService(
        IStatutoryRuleProvider ruleProvider,
        IPFCalculator pfCalculator,
        IESICalculator esiCalculator,
        IProfessionalTaxCalculator ptCalculator,
        ITDSCalculator tdsCalculator)
    {
        _ruleProvider = ruleProvider;
        _pfCalculator = pfCalculator;
        _esiCalculator = esiCalculator;
        _ptCalculator = ptCalculator;
        _tdsCalculator = tdsCalculator;
    }

    public async Task<StatutoryDeductionsResult> CalculateStatutoryAsync(
        int organizationId,
        int employeeId,
        string stateCode,
        decimal basicSalary,
        decimal grossSalary,
        int year,
        int month,
        DateOnly calculationDate)
    {
        var pfConfig = await _ruleProvider.GetPFConfigAsync(organizationId, calculationDate);
        var esiConfig = await _ruleProvider.GetESIConfigAsync(organizationId, calculationDate);
        var ptConfig = await _ruleProvider.GetPTConfigAsync(organizationId, stateCode, calculationDate);
        var tdsConfig = await _ruleProvider.GetTDSConfigAsync(organizationId, calculationDate);

        var pfResult = _pfCalculator.Calculate(basicSalary, pfConfig);
        var esiResult = _esiCalculator.Calculate(grossSalary, esiConfig);
        var ptResult = _ptCalculator.Calculate(grossSalary, month, ptConfig);

        // Compute remaining fiscal months (Indian Fiscal Year runs April 1 to March 31)
        // If month is 4 (April), remaining = 12. If month is 3 (March), remaining = 1.
        var fiscalMonthIndex = month >= 4 ? month - 3 : month + 9;
        var remainingFiscalMonths = Math.Max(1, 13 - fiscalMonthIndex);

        var tdsResult = _tdsCalculator.Calculate(
            monthlyTaxableGross: grossSalary,
            month: month,
            fiscalYearRemainingMonths: remainingFiscalMonths,
            yearToDateTaxPaid: 0m, // Extended with historical YTD if available
            declaredDeductions: 0m,
            config: tdsConfig
        );

        var items = new List<PayrollItem>();
        decimal totalEmployeeDeductions = 0m;
        decimal totalEmployerContributions = 0m;

        // 1. Employee PF
        if (pfResult.EmployeePF > 0)
        {
            items.Add(new PayrollItem
            {
                ComponentCode = "PF_EE",
                ComponentName = "Provident Fund (Employee)",
                ComponentType = ComponentType.Deduction,
                CalculationOrder = 5,
                CalculationType = ComponentCalculationType.PercentageOfBasic,
                CalculationBase = basicSalary,
                CalculationRate = pfConfig.EmployeeRate,
                OriginalAmount = pfResult.EmployeePF,
                ProratedAmount = pfResult.EmployeePF,
                FinalAmount = pfResult.EmployeePF,
                CalculationFormula = pfResult.Formula,
                CalculationNotes = pfResult.Notes
            });
            totalEmployeeDeductions += pfResult.EmployeePF;
        }

        // 2. Employer PF (Contribution)
        if (pfResult.TotalEmployerPF > 0)
        {
            items.Add(new PayrollItem
            {
                ComponentCode = "PF_ER",
                ComponentName = "Provident Fund (Employer)",
                ComponentType = ComponentType.EmployerContribution,
                CalculationOrder = 5,
                CalculationType = ComponentCalculationType.PercentageOfBasic,
                CalculationBase = basicSalary,
                CalculationRate = pfConfig.EmployerEPSRate + pfConfig.EmployerEPFRate,
                OriginalAmount = pfResult.TotalEmployerPF,
                ProratedAmount = pfResult.TotalEmployerPF,
                FinalAmount = pfResult.TotalEmployerPF,
                CalculationFormula = $"EPS: {pfResult.EmployerEPS} + EPF: {pfResult.EmployerEPF}",
                CalculationNotes = "Statutory employer contribution to EPF and EPS"
            });
            totalEmployerContributions += pfResult.TotalEmployerPF;
        }

        // 3. Employee ESI
        if (esiResult.IsApplicable && esiResult.EmployeeESI > 0)
        {
            items.Add(new PayrollItem
            {
                ComponentCode = "ESI_EE",
                ComponentName = "Employee State Insurance",
                ComponentType = ComponentType.Deduction,
                CalculationOrder = 6,
                CalculationType = ComponentCalculationType.PercentageOfGross,
                CalculationBase = grossSalary,
                CalculationRate = esiConfig.EmployeeRate,
                OriginalAmount = esiResult.EmployeeESI,
                ProratedAmount = esiResult.EmployeeESI,
                FinalAmount = esiResult.EmployeeESI,
                CalculationFormula = esiResult.Formula,
                CalculationNotes = esiResult.Notes
            });
            totalEmployeeDeductions += esiResult.EmployeeESI;
        }

        // 4. Employer ESI
        if (esiResult.IsApplicable && esiResult.EmployerESI > 0)
        {
            items.Add(new PayrollItem
            {
                ComponentCode = "ESI_ER",
                ComponentName = "ESI (Employer Contribution)",
                ComponentType = ComponentType.EmployerContribution,
                CalculationOrder = 6,
                CalculationType = ComponentCalculationType.PercentageOfGross,
                CalculationBase = grossSalary,
                CalculationRate = esiConfig.EmployerRate,
                OriginalAmount = esiResult.EmployerESI,
                ProratedAmount = esiResult.EmployerESI,
                FinalAmount = esiResult.EmployerESI,
                CalculationFormula = esiResult.Formula,
                CalculationNotes = esiResult.Notes
            });
            totalEmployerContributions += esiResult.EmployerESI;
        }

        // 5. Professional Tax
        if (ptResult.PTAmount > 0)
        {
            items.Add(new PayrollItem
            {
                ComponentCode = "PT",
                ComponentName = $"Professional Tax ({ptResult.StateCode})",
                ComponentType = ComponentType.Deduction,
                CalculationOrder = 7,
                CalculationType = ComponentCalculationType.FixedAmount,
                CalculationBase = grossSalary,
                CalculationRate = 0m,
                OriginalAmount = ptResult.PTAmount,
                ProratedAmount = ptResult.PTAmount,
                FinalAmount = ptResult.PTAmount,
                CalculationFormula = ptResult.Formula,
                CalculationNotes = ptResult.Notes
            });
            totalEmployeeDeductions += ptResult.PTAmount;
        }

        // 6. Tax Deducted at Source (TDS)
        if (tdsResult.MonthlyTDS > 0)
        {
            items.Add(new PayrollItem
            {
                ComponentCode = "TDS",
                ComponentName = "Income Tax (TDS)",
                ComponentType = ComponentType.Deduction,
                CalculationOrder = 8,
                CalculationType = ComponentCalculationType.Formula,
                CalculationBase = grossSalary,
                CalculationRate = 0m,
                OriginalAmount = tdsResult.MonthlyTDS,
                ProratedAmount = tdsResult.MonthlyTDS,
                FinalAmount = tdsResult.MonthlyTDS,
                CalculationFormula = tdsResult.Formula,
                CalculationNotes = tdsResult.Notes
            });
            totalEmployeeDeductions += tdsResult.MonthlyTDS;
        }

        return new StatutoryDeductionsResult(
            PF: pfResult,
            ESI: esiResult,
            PT: ptResult,
            TDS: tdsResult,
            TotalEmployeeDeductions: totalEmployeeDeductions,
            TotalEmployerContributions: totalEmployerContributions,
            GeneratedItems: items
        );
    }
}
