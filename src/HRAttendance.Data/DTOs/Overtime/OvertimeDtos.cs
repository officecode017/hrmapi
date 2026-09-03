namespace HRAttendance.Data.DTOs.Overtime;

public class OTEntryDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateOnly OTDate { get; set; }
    public decimal OTHours { get; set; }
    public decimal MultiplierApplied { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal OTAmount { get; set; }
}

public class CalculateOvertimeRequestDto
{
    public int EmployeeId { get; set; }
    public DateOnly Date { get; set; }
    public decimal HoursWorked { get; set; }
}
