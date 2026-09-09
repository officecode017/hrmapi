using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Interfaces.Payroll;

public record ComponentProration(
    int SalaryComponentId,
    string ComponentCode,
    string ComponentName,
    ComponentType Type,
    int CalculationOrder,
    ComponentCalculationType CalculationType,
    decimal FullMonthlyAmount,
    decimal ProratedAmount,
    string DerivationFormula,
    string DerivationNotes
);

public record ProrationResult(
    IReadOnlyList<PayrollSalarySlice> Slices,
    IReadOnlyList<ComponentProration> ComponentProrations,
    decimal BaseMonthlyGross,
    decimal ProratedGross
);

public interface IMidMonthProrationService
{
    ProrationResult CalculateSlices(
        EmploymentWindow window,
        IReadOnlyList<EmployeeSalaryStructure> structures,
        PayrollPolicy policy
    );
}
