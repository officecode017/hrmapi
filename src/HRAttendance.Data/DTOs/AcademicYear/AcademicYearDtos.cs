namespace HRAttendance.Data.DTOs.AcademicYear;

public class AcademicYearDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; }
}

public class CreateAcademicYearDto
{
    public int OrganizationId { get; set; } = 1;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateAcademicYearDto
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; }
}
