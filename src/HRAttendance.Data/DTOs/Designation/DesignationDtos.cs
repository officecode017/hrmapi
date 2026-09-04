namespace HRAttendance.Data.DTOs.Designation;

public class DesignationDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? Level { get; set; }
}

public class CreateDesignationDto
{
    public int OrganizationId { get; set; } = 1;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? Level { get; set; }
}

public class UpdateDesignationDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? Level { get; set; }
}
