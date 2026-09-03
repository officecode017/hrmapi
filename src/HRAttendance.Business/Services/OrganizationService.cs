using Microsoft.EntityFrameworkCore;
using HRAttendance.Business.Interfaces;
using HRAttendance.Data;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Organization;

namespace HRAttendance.Business.Services;

public class OrganizationService : IOrganizationService
{
    private readonly ApplicationDbContext _context;

    public OrganizationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponseDto<OrganizationDto>> GetOrganizationByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var org = await _context.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (org == null)
        {
            return ApiResponseDto<OrganizationDto>.Fail("Organization not found.");
        }

        var dto = new OrganizationDto
        {
            Id = org.Id,
            Name = org.Name,
            Phone = org.Phone,
            Email = org.Email,
            Website = org.Website,
            City = org.City,
            Country = org.Country
        };

        return ApiResponseDto<OrganizationDto>.Ok(dto);
    }

    public async Task<ApiResponseDto<List<DepartmentDto>>> GetDepartmentsAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        var depts = await _context.Departments
            .AsNoTracking()
            .Where(d => d.OrganizationId == organizationId)
            .Include(d => d.DepartmentHead)
            .ToListAsync(cancellationToken);

        var dtos = depts.Select(d => new DepartmentDto
        {
            Id = d.Id,
            OrganizationId = d.OrganizationId,
            Name = d.Name,
            DepartmentHeadId = d.DepartmentHeadId,
            DepartmentHeadName = d.DepartmentHead != null ? $"{d.DepartmentHead.FirstName} {d.DepartmentHead.LastName}".Trim() : null
        }).ToList();

        return ApiResponseDto<List<DepartmentDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<List<DesignationDto>>> GetDesignationsAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        var desigs = await _context.Designations
            .AsNoTracking()
            .Where(d => d.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        var dtos = desigs.Select(d => new DesignationDto
        {
            Id = d.Id,
            OrganizationId = d.OrganizationId,
            Name = d.Name
        }).ToList();

        return ApiResponseDto<List<DesignationDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<List<LocationDto>>> GetLocationsAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        var locs = await _context.Locations
            .AsNoTracking()
            .Where(l => l.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        var dtos = locs.Select(l => new LocationDto
        {
            Id = l.Id,
            OrganizationId = l.OrganizationId,
            Name = l.Name,
            Country = l.Country,
            Latitude = l.Latitude,
            Longitude = l.Longitude,
            Radius = l.Radius
        }).ToList();

        return ApiResponseDto<List<LocationDto>>.Ok(dtos);
    }
}
