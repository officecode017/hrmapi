using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;

namespace HRAttendance.Data.Models.Employee;

public class EmployeeContactDetails : AuditableEntity
{
    public int OrganizationId { get; set; }
    public int EmployeeId { get; set; }
    public string? Address { get; set; }
    public string? PermanentAddress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Mobile { get; set; }
    public string? WorkTelephone { get; set; }
    public string? HomeTelephone { get; set; }
    public string? Extension { get; set; }
    public string? WorkEmail { get; set; }
    public string? OtherEmail { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPerson { get; set; }
    public decimal? HomeLatitude { get; set; }
    public decimal? HomeLongitude { get; set; }

    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
}
