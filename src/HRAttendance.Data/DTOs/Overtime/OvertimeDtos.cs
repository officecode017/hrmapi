namespace HRAttendance.Data.DTOs.Overtime;

public class OTSettingDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsOverTimeEnabled { get; set; }
    public int OTStartAfterMinutes { get; set; }
    public decimal Multiplier { get; set; }
    public decimal MaxOTHoursPerDay { get; set; }
    public bool IsActive { get; set; }
}

public class CreateOTSettingDto
{
    public int OrganizationId { get; set; } = 1;
    public string Name { get; set; } = string.Empty;
    public bool IsOverTimeEnabled { get; set; } = true;
    public int OTStartAfterMinutes { get; set; } = 30;
    public decimal Multiplier { get; set; } = 1.50m;
    public decimal MaxOTHoursPerDay { get; set; } = 4.00m;
    public bool IsActive { get; set; } = true;
}

public class UpdateOTSettingDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsOverTimeEnabled { get; set; }
    public int OTStartAfterMinutes { get; set; }
    public decimal Multiplier { get; set; }
    public decimal MaxOTHoursPerDay { get; set; }
    public bool IsActive { get; set; }
}

public class OTEntryDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int OTSettingId { get; set; }
    public string? OTSettingName { get; set; }
    public DateOnly OTDate { get; set; }
    public decimal OTHours { get; set; }
    public decimal MultiplierApplied { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal OTAmount { get; set; }
    public string Status { get; set; } = "Pending";
    public string? ApprovedByName { get; set; }
}

public class CreateOTEntryDto
{
    public int EmployeeId { get; set; }
    public int OTSettingId { get; set; }
    public DateOnly OTDate { get; set; }
    public decimal OTHours { get; set; }
    public decimal HourlyRate { get; set; }
}

public class ApproveOTEntryDto
{
    public bool IsApproved { get; set; }
    public string? Remarks { get; set; }
}
