using HRAttendance.Data.Models.Common;

namespace HRAttendance.Data.Models.Payroll;

public class PayrollItem : AuditableEntity
{
    public int PayrollEmployeeId { get; set; }
    public string ComponentCode { get; set; } = string.Empty;
    public string ComponentName { get; set; } = string.Empty;
    public ComponentType ComponentType { get; set; }
    public int CalculationOrder { get; set; }
    public ComponentCalculationType CalculationType { get; set; }
    public decimal CalculationBase { get; set; }
    public decimal CalculationRate { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal ProratedAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string CalculationFormula { get; set; } = string.Empty;
    public string CalculationNotes { get; set; } = string.Empty;

    public virtual PayrollEmployee PayrollEmployee { get; set; } = null!;
}
