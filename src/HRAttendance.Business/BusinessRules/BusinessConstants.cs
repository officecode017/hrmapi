namespace HRAttendance.Business.BusinessRules;

public static class ApplicationRoles
{
    public const string SuperAdmin = "Super Admin";
    public const string Admin = "HR/Admin";
    public const string Manager = "Manager";
    public const string Employee = "Employee";

    public static readonly string[] All = { SuperAdmin, Admin, Manager, Employee };
}

public enum AttendanceStatus
{
    Present = 1,
    Late = 2,
    HalfDay = 3,
    Absent = 4,
    OnLeave = 5,
    Holiday = 6,
    Weekend = 7
}

public static class LeaveStatus
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";
}
