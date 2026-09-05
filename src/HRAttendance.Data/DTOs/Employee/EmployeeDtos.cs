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
    public string? PhotoPath { get; set; }
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
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
    public string? Nationality { get; set; }
    public string? Religion { get; set; }
    public string? BirthPlace { get; set; }
    public string? IdentificationMark { get; set; }
    public string? EmployeeType { get; set; }
    public string? Qualification { get; set; }
    public string? SkillSet { get; set; }
    public string? PhotoPath { get; set; }
    public string? BackgroundImagePath { get; set; }
    public bool IsActive { get; set; }
    public DateOnly? ResignationDate { get; set; }
    public DateOnly? LastWorkingDay { get; set; }
    public string? ReasonForLeaving { get; set; }

    // Contact Details
    public string? Address { get; set; }
    public string? PermanentAddress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? WorkEmail { get; set; }
    public string? OtherEmail { get; set; }
    public string? Mobile { get; set; }
    public string? WorkTelephone { get; set; }
    public string? HomeTelephone { get; set; }
    public string? Extension { get; set; }
    public string? EmergencyPerson { get; set; }
    public string? EmergencyContact { get; set; }

    // Professional Details
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
    public int? ProbationPeriodMonths { get; set; }
    public DateOnly? ConfirmationDate { get; set; }
    public DateOnly? ContractEndDate { get; set; }
    public int? NoticePeriodDays { get; set; }

    public List<string> Roles { get; set; } = new();
    public List<int> RoleIds { get; set; } = new();
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
    public string? BloodGroup { get; set; }
    public string? MaritalStatus { get; set; }
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
    public string? Nationality { get; set; }
    public string? Religion { get; set; }
    public string? BirthPlace { get; set; }
    public string? IdentificationMark { get; set; }
    public string? EmployeeType { get; set; } = "Full-Time";
    public string? Qualification { get; set; }
    public string? SkillSet { get; set; }
    public string? PhotoPath { get; set; }
    public string? BackgroundImagePath { get; set; }
    public string Password { get; set; } = string.Empty;


    // Contact
    public string WorkEmail { get; set; } = string.Empty;
    public string? OtherEmail { get; set; }
    public string? Mobile { get; set; }
    public string? WorkTelephone { get; set; }
    public string? HomeTelephone { get; set; }
    public string? Extension { get; set; }
    public string? Address { get; set; }
    public string? PermanentAddress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? EmergencyPerson { get; set; }
    public string? EmergencyContact { get; set; }

    // Professional
    public int? DepartmentId { get; set; }
    public int? DesignationId { get; set; }
    public int? LocationId { get; set; }
    public int? ShiftId { get; set; }
    public int? ReportingTo { get; set; }
    public DateOnly? DateOfJoining { get; set; }
    public int? ProbationPeriodMonths { get; set; }
    public DateOnly? ConfirmationDate { get; set; }
    public DateOnly? ContractEndDate { get; set; }
    public int? NoticePeriodDays { get; set; }
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
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
    public string? Nationality { get; set; }
    public string? Religion { get; set; }
    public string? BirthPlace { get; set; }
    public string? IdentificationMark { get; set; }
    public string? EmployeeType { get; set; }
    public string? Qualification { get; set; }
    public string? SkillSet { get; set; }
    public bool IsActive { get; set; }
    public DateOnly? ResignationDate { get; set; }
    public DateOnly? LastWorkingDay { get; set; }
    public string? ReasonForLeaving { get; set; }

    // Contact
    public string? WorkEmail { get; set; }
    public string? OtherEmail { get; set; }
    public string? Mobile { get; set; }
    public string? WorkTelephone { get; set; }
    public string? HomeTelephone { get; set; }
    public string? Extension { get; set; }
    public string? Address { get; set; }
    public string? PermanentAddress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? EmergencyPerson { get; set; }
    public string? EmergencyContact { get; set; }

    // Professional
    public int? DepartmentId { get; set; }
    public int? DesignationId { get; set; }
    public int? LocationId { get; set; }
    public int? ShiftId { get; set; }
    public int? ReportingTo { get; set; }
    public DateOnly? DateOfJoining { get; set; }
    public int? ProbationPeriodMonths { get; set; }
    public DateOnly? ConfirmationDate { get; set; }
    public DateOnly? ContractEndDate { get; set; }
    public int? NoticePeriodDays { get; set; }
    public List<int>? RoleIds { get; set; }
    public string? Password { get; set; }
    public string? PhotoPath { get; set; }
    public string? BackgroundImagePath { get; set; }
}

public class EmployeePhotoResponseDto
{
    public string PhotoPath { get; set; } = string.Empty;
}

public class EmployeeBackgroundResponseDto
{
    public string BackgroundImagePath { get; set; } = string.Empty;
}
