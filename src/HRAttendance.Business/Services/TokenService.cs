using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using HRAttendance.Business.Interfaces;
using HRAttendance.Data.Models.Employee;

namespace HRAttendance.Business.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Employee employee, List<string> roles, out DateTime expiration)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "DefaultFallbackSecretKeyForDevelopmentAndTestingPurposeOnly12345!";
        var issuer = _configuration["Jwt:Issuer"] ?? "HRAttendance";
        var audience = _configuration["Jwt:Audience"] ?? "HRAttendanceApp";
        var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var minutes) ? minutes : 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        expiration = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, employee.Id.ToString()),
            new(ClaimTypes.Name, $"{employee.FirstName} {employee.LastName}".Trim()),
            new("OrganizationId", employee.OrganizationId.ToString()),
            new("EmployeeCode", employee.EmployeeCode),
            new(ClaimTypes.Email, employee.ContactDetails?.WorkEmail ?? $"{employee.EmployeeCode}@hrm.local")
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
