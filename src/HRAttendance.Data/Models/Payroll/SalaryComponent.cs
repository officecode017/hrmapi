using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;

namespace HRAttendance.Data.Models.Payroll;

public class SalaryComponent : AuditableEntity
{
    public int OrganizationId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ComponentType Type { get; set; }
    public ComponentCalculationType CalculationType { get; set; }
    public int CalculationOrder { get; set; }
    public bool IsTaxable { get; set; } = true;
    public bool IsStatutory { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual ICollection<EmployeeSalaryStructureItem> StructureItems { get; set; } = new List<EmployeeSalaryStructureItem>();
}
