namespace HRAttendance.Data.DTOs.Employee;

public class EmployeeListDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? WorkEmail { get; set; }
    public string? Mobile { get; set; }
    public string? DepartmentName { get; set; }
    public string? DesignationName { get; set; }
    public bool IsActive { get; set; }
}

public class EmployeeDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
    public DateOnly? DOB { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public string? MaritalStatus { get; set; }
    public bool IsActive { get; set; }

    // Contact
    public string? WorkEmail { get; set; }
    public string? Mobile { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }

    // Professional
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? DesignationId { get; set; }
    public string? DesignationName { get; set; }
    public int? LocationId { get; set; }
    public string? LocationName { get; set; }
    public int? ShiftId { get; set; }
    public string? ShiftName { get; set; }
    public int? ReportingTo { get; set; }
    public string? ReportingToName { get; set; }
    public DateOnly? DateOfJoining { get; set; }

    public List<string> Roles { get; set; } = new();
}

public class CreateEmployeeDto
{
    public int OrganizationId { get; set; } = 1;
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public DateOnly? DOB { get; set; }
    public string? Gender { get; set; }
    public string Password { get; set; } = "P@ssword123";

    // Contact
    public string WorkEmail { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }

    // Professional
    public int? DepartmentId { get; set; }
    public int? DesignationId { get; set; }
    public int? LocationId { get; set; }
    public int? ShiftId { get; set; }
    public int? ReportingTo { get; set; }
    public DateOnly? DateOfJoining { get; set; }
    public List<int> RoleIds { get; set; } = new();
}

public class UpdateEmployeeDto
{
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public DateOnly? DOB { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public string? MaritalStatus { get; set; }
    public bool IsActive { get; set; }

    // Contact
    public string? WorkEmail { get; set; }
    public string? Mobile { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }

    // Professional
    public int? DepartmentId { get; set; }
    public int? DesignationId { get; set; }
    public int? LocationId { get; set; }
    public int? ShiftId { get; set; }
    public int? ReportingTo { get; set; }
}
