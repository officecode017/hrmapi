namespace HRAttendance.Data.DTOs.Auth;

public class LoginRequestDto
{
    public int OrganizationId { get; set; } = 1;
    public string EmployeeCodeOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public UserSessionDto User { get; set; } = null!;
}

public class UserSessionDto
{
    public int EmployeeId { get; set; }
    public int OrganizationId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}
