using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HRAttendance.Business.Interfaces;
using HRAttendance.Data;
using HRAttendance.Data.DTOs.Auth;
using HRAttendance.Data.DTOs.Common;

namespace HRAttendance.Business.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext context,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        ILogger<AuthService> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<ApiResponseDto<LoginResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var identifier = request.EmployeeCodeOrEmail.Trim();

        var employee = await _context.Employees
            .Include(e => e.ContactDetails)
            .Include(e => e.EmployeeRoles)
                .ThenInclude(er => er.Role)
            .FirstOrDefaultAsync(e => e.OrganizationId == request.OrganizationId 
                                   && (e.EmployeeCode == identifier || (e.ContactDetails != null && e.ContactDetails.WorkEmail == identifier)), 
                                 cancellationToken);

        if (employee == null)
        {
            _logger.LogWarning("Authentication failed: User with identifier {Identifier} not found in Organization {OrgId}", identifier, request.OrganizationId);
            return ApiResponseDto<LoginResponseDto>.Fail("Invalid credentials.");
        }

        if (!employee.IsActive)
        {
            _logger.LogWarning("Authentication failed: User {EmployeeId} is inactive", employee.Id);
            return ApiResponseDto<LoginResponseDto>.Fail("Account is inactive. Please contact your administrator.");
        }

        // Verify password
        if (string.IsNullOrEmpty(employee.PasswordHash) || !_passwordHasher.VerifyPassword(request.Password, employee.PasswordHash))
        {
            _logger.LogWarning("Authentication failed: Password mismatch for employee {EmployeeId}", employee.Id);
            return ApiResponseDto<LoginResponseDto>.Fail("Invalid credentials.");
        }

        var roles = employee.EmployeeRoles
            .Where(r => r.Role != null && r.Role.IsActive)
            .Select(r => r.Role.Name)
            .ToList();

        if (!roles.Any())
        {
            roles.Add("Employee");
        }

        var token = _tokenService.GenerateToken(employee, roles, out var expiration);

        _logger.LogInformation("Authentication successful for employee {EmployeeId} ({Code})", employee.Id, employee.EmployeeCode);

        var response = new LoginResponseDto
        {
            Token = token,
            Expiration = expiration,
            User = new UserSessionDto
            {
                EmployeeId = employee.Id,
                OrganizationId = employee.OrganizationId,
                EmployeeCode = employee.EmployeeCode,
                FullName = $"{employee.FirstName} {employee.LastName}".Trim(),
                Email = employee.ContactDetails?.WorkEmail ?? $"{employee.EmployeeCode}@hrm.local",
                Roles = roles
            }
        };

        return ApiResponseDto<LoginResponseDto>.Ok(response, "Login successful.");
    }

    public async Task<ApiResponseDto<UserSessionDto>> GetCurrentUserAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await _context.Employees
            .Include(e => e.ContactDetails)
            .Include(e => e.EmployeeRoles)
                .ThenInclude(er => er.Role)
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);

        if (employee == null)
        {
            return ApiResponseDto<UserSessionDto>.Fail("User not found.");
        }

        var roles = employee.EmployeeRoles
            .Where(r => r.Role != null && r.Role.IsActive)
            .Select(r => r.Role.Name)
            .ToList();

        var session = new UserSessionDto
        {
            EmployeeId = employee.Id,
            OrganizationId = employee.OrganizationId,
            EmployeeCode = employee.EmployeeCode,
            FullName = $"{employee.FirstName} {employee.LastName}".Trim(),
            Email = employee.ContactDetails?.WorkEmail ?? $"{employee.EmployeeCode}@hrm.local",
            Roles = roles
        };

        return ApiResponseDto<UserSessionDto>.Ok(session);
    }
}
