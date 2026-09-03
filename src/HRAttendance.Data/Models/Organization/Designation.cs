using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Employee;

namespace HRAttendance.Data.Models.Organization;

public class Designation : AuditableEntity
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;

    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<EmployeeProfessionalDetails> EmployeeProfessionalDetails { get; set; } = new List<EmployeeProfessionalDetails>();
}
