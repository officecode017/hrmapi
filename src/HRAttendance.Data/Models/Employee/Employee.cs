using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Security;
using HRAttendance.Data.Models.Attendance;
using HRAttendance.Data.Models.Leave;
using HRAttendance.Data.Models.Overtime;
using HRAttendance.Data.Models.Notification;

namespace HRAttendance.Data.Models.Employee;

public class Employee : AuditableEntity
{
    public int OrganizationId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
    public DateOnly? DOB { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Nationality { get; set; }
    public string? Religion { get; set; }
    public string? BirthPlace { get; set; }
    public string? IdentificationMark { get; set; }
    public string? PhotoPath { get; set; }
    public string? EmployeeType { get; set; }
    public string? Qualification { get; set; }
    public string? SkillSet { get; set; }
    public string? DeviceId { get; set; }
    public string? GSMid { get; set; }
    public string? TranscardId { get; set; }
    public DateTime? LastSeen { get; set; }
    public string? UserStatus { get; set; }
    public string? EmployeeStatusMessage { get; set; }
    public string? EmployeeLiveMessage { get; set; }
    public bool IsGenericAccount { get; set; }
    public bool ProfileUpdated { get; set; }
    public bool IsActive { get; set; }
    public DateOnly? ResignationDate { get; set; }
    public DateOnly? LastWorkingDay { get; set; }
    public string? ReasonForLeaving { get; set; }
    
    // Auth password hash for JWT authentication
    public string? PasswordHash { get; set; }

    // Navigation
    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual EmployeeContactDetails? ContactDetails { get; set; }
    public virtual EmployeeProfessionalDetails? ProfessionalDetails { get; set; }
    public virtual ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();
    public virtual ICollection<EmployeeAttendance> EmployeeAttendances { get; set; } = new List<EmployeeAttendance>();
    public virtual ICollection<LeaveApplication> LeaveApplications { get; set; } = new List<LeaveApplication>();
    public virtual ICollection<SubLeaveApplication> SubLeaveApplications { get; set; } = new List<SubLeaveApplication>();
    public virtual ICollection<EmployeeLeave> EmployeeLeaves { get; set; } = new List<EmployeeLeave>();
    public virtual ICollection<OTEntry> OTEntries { get; set; } = new List<OTEntry>();
    public virtual ICollection<Models.Notification.Notification> Notifications { get; set; } = new List<Models.Notification.Notification>();
    public virtual ICollection<Models.Notification.Notification> SubjectOfNotifications { get; set; } = new List<Models.Notification.Notification>();
    public virtual ICollection<EmployeeProfessionalDetails> DirectReports { get; set; } = new List<EmployeeProfessionalDetails>();
    public virtual ICollection<Department> HeadedDepartments { get; set; } = new List<Department>();
}
