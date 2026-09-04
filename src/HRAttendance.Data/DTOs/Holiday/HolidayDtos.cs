namespace HRAttendance.Data.DTOs.Holiday;

public class HolidayDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int AcademicYearId { get; set; }
    public int? LocationId { get; set; }
    public string? LocationName { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public bool IsOptional { get; set; }
}

public class CreateHolidayDto
{
    public int OrganizationId { get; set; } = 1;
    public int AcademicYearId { get; set; } = 1;
    public int? LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public bool IsOptional { get; set; } = false;
}

public class UpdateHolidayDto
{
    public int? LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public bool IsOptional { get; set; }
}
