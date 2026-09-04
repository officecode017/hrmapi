namespace HRAttendance.Data.DTOs.Location;

public class LocationDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? EmailAlias { get; set; }
    public string? ContactNumber { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int? Radius { get; set; }
    public string? TimeZone { get; set; }
    public string? TimeZoneValue { get; set; }
}

public class CreateLocationDto
{
    public int OrganizationId { get; set; } = 1;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? EmailAlias { get; set; }
    public string? ContactNumber { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int? Radius { get; set; } = 500;
    public string? TimeZone { get; set; }
    public string? TimeZoneValue { get; set; }
}

public class UpdateLocationDto
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? EmailAlias { get; set; }
    public string? ContactNumber { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int? Radius { get; set; }
    public string? TimeZone { get; set; }
    public string? TimeZoneValue { get; set; }
}
