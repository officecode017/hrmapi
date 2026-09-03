using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;

namespace HRAttendance.Data.Models.Employee;

public class EmployeeProfessionalDetails : AuditableEntity
{
    public int OrganizationId { get; set; }
    public int EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public int? DesignationId { get; set; }
    public int? LocationId { get; set; }
    public int? ShiftId { get; set; }
    public int? ReportingTo { get; set; }
    public int? TeamId { get; set; }
    public string? Grade { get; set; }
    public DateOnly? DateOfJoining { get; set; }
    public string? NatureOfPost { get; set; }
    public int? ProbationPeriod { get; set; }
    public DateOnly? PromotionDate { get; set; }
    public DateOnly? SalaryHikeDate { get; set; }
    public DateOnly? UniformIssuedDate { get; set; }
    public string? ReferenceName { get; set; }

    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
    public virtual Department? Department { get; set; }
    public virtual Designation? Designation { get; set; }
    public virtual Location? Location { get; set; }
    public virtual Shift? Shift { get; set; }
    public virtual Employee? Manager { get; set; }
}
