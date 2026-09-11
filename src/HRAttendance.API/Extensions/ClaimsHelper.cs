using System.Security.Claims;
using HRAttendance.Business.BusinessRules;

namespace HRAttendance.API.Extensions;

public static class ClaimsHelper
{
    /// <summary>
    /// Gets the authenticated employee's ID from the JWT NameIdentifier claim.
    /// </summary>
    public static int GetEmployeeId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(claim) || !int.TryParse(claim, out var employeeId))
        {
            throw new UnauthorizedAccessException("Employee ID could not be identified from token claims.");
        }
        return employeeId;
    }

    /// <summary>
    /// Try to get the authenticated employee ID, returning null if not found.
    /// </summary>
    public static int? TryGetEmployeeId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }

    /// <summary>
    /// Gets the Organization ID from the token claim.
    /// </summary>
    public static int GetOrganizationId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst("OrganizationId")?.Value;
        if (string.IsNullOrWhiteSpace(claim) || !int.TryParse(claim, out var organizationId))
        {
            throw new UnauthorizedAccessException("Organization ID could not be identified from token claims.");
        }
        return organizationId;
    }

    /// <summary>
    /// Gets the Organization ID from token claim, or falls back to defaultValue (e.g. 1).
    /// </summary>
    public static int GetOrganizationIdOrDefault(this ClaimsPrincipal user, int defaultValue = 1)
    {
        var claim = user.FindFirst("OrganizationId")?.Value;
        return (int.TryParse(claim, out var id) && id > 0) ? id : defaultValue;
    }

    /// <summary>
    /// Gets the employee code from the token claim.
    /// </summary>
    public static string GetEmployeeCode(this ClaimsPrincipal user)
    {
        return user.FindFirst("EmployeeCode")?.Value ?? string.Empty;
    }

    /// <summary>
    /// Gets the email address from the token claim.
    /// </summary>
    public static string GetEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
    }

    /// <summary>
    /// Checks if the user holds Manager, HR/Admin, or SuperAdmin roles.
    /// </summary>
    public static bool IsManagerOrAdmin(this ClaimsPrincipal user)
    {
        return user.IsInRole(ApplicationRoles.SuperAdmin) ||
               user.IsInRole(ApplicationRoles.Admin) ||
               user.IsInRole(ApplicationRoles.Manager);
    }
}
