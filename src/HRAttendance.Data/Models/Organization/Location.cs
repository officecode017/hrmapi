using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Employee;
using HRAttendance.Data.Models.Attendance;

namespace HRAttendance.Data.Models.Organization;

public class Location : AuditableEntity
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? EmailAlias { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int? Radius { get; set; }
    public int? TimeZone { get; set; }
    public string? TimeZoneValue { get; set; }

    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<Holiday> Holidays { get; set; } = new List<Holiday>();
    public virtual ICollection<OffDay> OffDays { get; set; } = new List<OffDay>();
    public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();
    public virtual ICollection<EmployeeProfessionalDetails> EmployeeProfessionalDetails { get; set; } = new List<EmployeeProfessionalDetails>();
    public virtual ICollection<EmployeeAttendance> EmployeeAttendances { get; set; } = new List<EmployeeAttendance>();
}
