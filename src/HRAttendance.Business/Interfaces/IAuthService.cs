using HRAttendance.Data.DTOs.Auth;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.Models.Employee;

namespace HRAttendance.Business.Interfaces;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}

public interface ITokenService
{
    string GenerateToken(Employee employee, List<string> roles, out DateTime expiration, List<string>? permissions = null);
}

public interface IAuthService
{
    Task<ApiResponseDto<LoginResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<UserSessionDto>> GetCurrentUserAsync(int employeeId, CancellationToken cancellationToken = default);
}
