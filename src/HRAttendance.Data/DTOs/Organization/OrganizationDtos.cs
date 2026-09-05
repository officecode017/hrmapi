namespace HRAttendance.Data.DTOs.Organization;

public class OrganizationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Industry { get; set; }
    public string? TaxId { get; set; }
    public string? LogoPath { get; set; }
    public string? LogoUrl { get; set; }
    public string? Currency { get; set; }
    public int? FiscalYearStartMonth { get; set; }
}

public class OrganizationBrandingDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Website { get; set; }
}

public class OrganizationLogoResponseDto
{
    public string LogoUrl { get; set; } = string.Empty;
}

public class CreateOrganizationDto
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Industry { get; set; }
    public string? TaxId { get; set; }
    public string? LogoPath { get; set; }
    public string? Currency { get; set; } = "USD";
    public int? FiscalYearStartMonth { get; set; } = 1;
}

public class UpdateOrganizationDto
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Industry { get; set; }
    public string? TaxId { get; set; }
    public string? LogoPath { get; set; }
    public string? Currency { get; set; }
    public int? FiscalYearStartMonth { get; set; }
}
