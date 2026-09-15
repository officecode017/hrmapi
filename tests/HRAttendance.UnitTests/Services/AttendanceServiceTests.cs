using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using HRAttendance.Business.BusinessRules;
using HRAttendance.Business.Services;
using HRAttendance.Data;
using HRAttendance.Data.DTOs.Attendance;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Employee;
using HRAttendance.Data.Repositories;
using Xunit;

namespace HRAttendance.UnitTests.Services;

public class AttendanceServiceTests
{
    private ApplicationDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public void ResolveTimeZone_WithValidWindowsId_ShouldResolveCorrectly()
    {
        var location = new Location
        {
            Name = "India HQ",
            TimeZoneValue = "India Standard Time"
        };

        var tz = AttendanceService.ResolveTimeZone(location);

        tz.Should().NotBeNull();
        tz.Id.Should().BeOneOf("India Standard Time", "Asia/Kolkata", "Asia/Calcutta");
    }

    [Fact]
    public void ResolveTimeZone_WithNullLocation_ShouldFallbackToIndiaOrLocal()
    {
        var tz = AttendanceService.ResolveTimeZone(null);

        tz.Should().NotBeNull();
    }

    [Fact]
    public void ResolveTimeZone_WithIanaId_ShouldResolveCorrectly()
    {
        var location = new Location
        {
            Name = "Kolkata Campus",
            TimeZoneValue = "Asia/Kolkata"
        };

        var tz = AttendanceService.ResolveTimeZone(location);

        tz.Should().NotBeNull();
        tz.Id.Should().BeOneOf("India Standard Time", "Asia/Kolkata", "Asia/Calcutta");
    }

    [Fact]
    public async Task CheckInAsync_WhenArrivingPastGracePeriod_ShouldMarkAsLate()
    {
        using var context = CreateDbContext(nameof(CheckInAsync_WhenArrivingPastGracePeriod_ShouldMarkAsLate));
        var repo = new AttendanceRepository(context);
        var service = new AttendanceService(repo, context, NullLogger<AttendanceService>.Instance);

        var location = new Location
        {
            Id = 1,
            OrganizationId = 1,
            Name = "Main Campus",
            TimeZoneValue = "India Standard Time"
        };
        await context.Locations.AddAsync(location);

        // Shift starts at 00:00:00 with 0 grace so current time in local is guaranteed > shift
        var shift = new Shift
        {
            Id = 1,
            OrganizationId = 1,
            LocationId = 1,
            Name = "Early Shift",
            InTime = new TimeOnly(0, 1),
            OutTime = new TimeOnly(8, 0),
            GraceMinutes = 0
        };
        await context.Shifts.AddAsync(shift);

        var academicYear = new AcademicYear
        {
            Id = 1,
            OrganizationId = 1,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(11)),
            IsActive = true
        };
        await context.AcademicYears.AddAsync(academicYear);

        var employee = new Employee
        {
            Id = 101,
            OrganizationId = 1,
            FirstName = "Rahul",
            LastName = "Sharma",
            EmployeeCode = "EMP101",
            ProfessionalDetails = new EmployeeProfessionalDetails
            {
                EmployeeId = 101,
                LocationId = 1,
                ShiftId = 1
            }
        };
        await context.Employees.AddAsync(employee);
        await context.SaveChangesAsync();

        var request = new CheckInRequestDto
        {
            EmployeeId = 101
        };

        var result = await service.CheckInAsync(request);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        // Since local time is well after 00:01 AM, status must be Late
        result.Data!.Status.Should().Be(AttendanceStatus.Late.ToString());
    }
}
