using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Employee;
using HRAttendance.Data.Models.Security;
using HRAttendance.Data.Models.Leave;
using HRAttendance.Data.Models.Attendance;
using HRAttendance.Data.Models.Overtime;
using HRAttendance.Data.Models.Notification;

namespace HRAttendance.Data.Models.Organization;

public class Organization : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Email { get; set; }
    public string? Fax { get; set; }
    public string? PoBox { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? ContactPerson { get; set; }
    public string? Industry { get; set; }

    // Navigation collections
    public virtual ICollection<AcademicYear> AcademicYears { get; set; } = new List<AcademicYear>();
    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
    public virtual ICollection<Designation> Designations { get; set; } = new List<Designation>();
    public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();
    public virtual ICollection<Holiday> Holidays { get; set; } = new List<Holiday>();
    public virtual ICollection<OffDay> OffDays { get; set; } = new List<OffDay>();
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    public virtual ICollection<PermissionMaster> PermissionMasters { get; set; } = new List<PermissionMaster>();
    public virtual ICollection<Models.Employee.Employee> Employees { get; set; } = new List<Models.Employee.Employee>();
    public virtual ICollection<LeaveType> LeaveTypes { get; set; } = new List<LeaveType>();
    public virtual ICollection<OTSetting> OTSettings { get; set; } = new List<OTSetting>();
    public virtual ICollection<EmployeeAttendance> EmployeeAttendances { get; set; } = new List<EmployeeAttendance>();
    public virtual ICollection<Models.Notification.Notification> Notifications { get; set; } = new List<Models.Notification.Notification>();
}
