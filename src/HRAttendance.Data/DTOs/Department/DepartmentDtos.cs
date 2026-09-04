namespace HRAttendance.Data.DTOs.Department;

public class DepartmentDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public int? DepartmentHeadId { get; set; }
    public string? DepartmentHeadName { get; set; }
}

public class CreateDepartmentDto
{
    public int OrganizationId { get; set; } = 1;
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public int? DepartmentHeadId { get; set; }
}

public class UpdateDepartmentDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public int? DepartmentHeadId { get; set; }
}
