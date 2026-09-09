using HRAttendance.Data.Models.Common;

namespace HRAttendance.Data.Models.Payroll;

public class EmployeeSalaryStructureItem : AuditableEntity
{
    public int EmployeeSalaryStructureId { get; set; }
    public int SalaryComponentId { get; set; }
    public decimal MonthlyAmount { get; set; }
    public decimal AnnualAmount { get; set; }
    public decimal PercentageRate { get; set; }

    public virtual EmployeeSalaryStructure Structure { get; set; } = null!;
    public virtual SalaryComponent Component { get; set; } = null!;
}
