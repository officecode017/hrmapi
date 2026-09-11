using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;

namespace HRAttendance.Data.Models.Payroll;

public class EmployeeSalaryStructure : AuditableEntity
{
    public int OrganizationId { get; set; }
    public int EmployeeId { get; set; }
    public int Version { get; set; } = 1;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public decimal MonthlyGrossSalary { get; set; }
    public decimal AnnualCTC { get; set; }
    public string? RevisionReason { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual Models.Employee.Employee Employee { get; set; } = null!;
    public virtual ICollection<EmployeeSalaryStructureItem> Items { get; set; } = new List<EmployeeSalaryStructureItem>();
    public virtual ICollection<PayrollSalarySlice> SalarySlices { get; set; } = new List<PayrollSalarySlice>();
}
