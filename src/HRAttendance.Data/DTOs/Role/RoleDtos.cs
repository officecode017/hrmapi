namespace HRAttendance.Data.DTOs.Role;

public class RoleDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class AssignRoleDto
{
    public int EmployeeId { get; set; }
    public int RoleId { get; set; }
}
