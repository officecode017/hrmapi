using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using HRAttendance.Business.BusinessRules;

namespace HRAttendance.API.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"] ?? "DefaultFallbackSecretKeyForDevelopmentAndTestingPurposeOnly12345!";
        var issuer = configuration["Jwt:Issuer"] ?? "HRAttendance";
        var audience = configuration["Jwt:Audience"] ?? "HRAttendanceApp";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // Dev friendly, can be enforced in Prod
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireSuperAdmin", policy => policy.RequireRole(ApplicationRoles.SuperAdmin));
            options.AddPolicy("RequireAdmin", policy => policy.RequireRole(ApplicationRoles.SuperAdmin, ApplicationRoles.Admin));
            options.AddPolicy("RequireManager", policy => policy.RequireRole(ApplicationRoles.SuperAdmin, ApplicationRoles.Admin, ApplicationRoles.Manager));
            options.AddPolicy("RequireEmployee", policy => policy.RequireRole(ApplicationRoles.All));
        });

        return services;
    }
}
