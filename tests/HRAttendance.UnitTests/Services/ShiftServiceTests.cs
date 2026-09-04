using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using HRAttendance.Business.Services;
using HRAttendance.Data;
using HRAttendance.Data.DTOs.Shift;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Employee;
using Xunit;

namespace HRAttendance.UnitTests.Services;

public class ShiftServiceTests
{
    private ApplicationDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateShiftAsync_ShouldAddShiftAndReturnDto()
    {
        using var context = CreateDbContext(nameof(CreateShiftAsync_ShouldAddShiftAndReturnDto));
        var service = new ShiftService(context);

        var dto = new CreateShiftDto
        {
            OrganizationId = 1,
            LocationId = 1,
            Name = "Morning Shift",
            InTime = new TimeOnly(9, 0),
            OutTime = new TimeOnly(18, 0),
            GraceMinutes = 15,
            BreakMinutes = 60,
            BreakStartTime = new TimeOnly(13, 0),
            BreakEndTime = new TimeOnly(14, 0)
        };

        var result = await service.CreateShiftAsync(dto);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Morning Shift");
        result.Data.GraceMinutes.Should().Be(15);
        result.Data.BreakMinutes.Should().Be(60);
        result.Data.BreakStartTime.Should().Be(new TimeOnly(13, 0));
        result.Data.BreakEndTime.Should().Be(new TimeOnly(14, 0));

        var inDb = await context.Shifts.FirstOrDefaultAsync(s => s.Id == result.Data.Id);
        inDb.Should().NotBeNull();
    }

    [Fact]
    public async Task AssignShiftAsync_WhenEmployeeExists_ShouldUpdateShiftId()
    {
        using var context = CreateDbContext(nameof(AssignShiftAsync_WhenEmployeeExists_ShouldUpdateShiftId));
        var employee = new Employee
        {
            Id = 10,
            OrganizationId = 1,
            EmployeeCode = "EMP010",
            FirstName = "Alice",
            LastName = "Smith"
        };
        var shift = new Shift
        {
            Id = 5,
            OrganizationId = 1,
            LocationId = 1,
            Name = "General Shift",
            InTime = new TimeOnly(9, 0),
            OutTime = new TimeOnly(17, 0)
        };
        await context.Employees.AddAsync(employee);
        await context.Shifts.AddAsync(shift);
        await context.SaveChangesAsync();

        var service = new ShiftService(context);
        var result = await service.AssignShiftAsync(new AssignShiftDto
        {
            EmployeeId = 10,
            ShiftId = 5
        });

        result.Success.Should().BeTrue();
        var details = await context.EmployeeProfessionalDetails.FirstOrDefaultAsync(p => p.EmployeeId == 10);
        details.Should().NotBeNull();
        details!.ShiftId.Should().Be(5);
    }

    [Fact]
    public async Task DeleteShiftAsync_ShouldSoftDeleteShift()
    {
        using var context = CreateDbContext(nameof(DeleteShiftAsync_ShouldSoftDeleteShift));
        var shift = new Shift
        {
            Id = 20,
            OrganizationId = 1,
            LocationId = 1,
            Name = "Temporary Shift",
            InTime = new TimeOnly(10, 0),
            OutTime = new TimeOnly(19, 0)
        };
        await context.Shifts.AddAsync(shift);
        await context.SaveChangesAsync();

        var service = new ShiftService(context);
        var result = await service.DeleteShiftAsync(20);

        result.Success.Should().BeTrue();
        var inDb = await context.Shifts.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == 20);
        inDb.Should().NotBeNull();
        inDb!.IsDeleted.Should().BeTrue();
    }
}
