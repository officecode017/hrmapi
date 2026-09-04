using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using HRAttendance.Business.Services;
using HRAttendance.Data;
using HRAttendance.Data.DTOs.Overtime;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Employee;
using HRAttendance.Data.Models.Overtime;
using Xunit;

namespace HRAttendance.UnitTests.Services;

public class OvertimeServiceTests
{
    private ApplicationDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateOTSettingAsync_ShouldPersistAndReturnDto()
    {
        using var context = CreateDbContext(nameof(CreateOTSettingAsync_ShouldPersistAndReturnDto));
        var service = new OvertimeService(context);

        var dto = new CreateOTSettingDto
        {
            OrganizationId = 1,
            Name = "Standard OT Rule",
            IsOverTimeEnabled = true,
            OTStartAfterMinutes = 30,
            Multiplier = 1.50m,
            MaxOTHoursPerDay = 4.00m
        };

        var result = await service.CreateOTSettingAsync(dto);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Standard OT Rule");
        result.Data.Multiplier.Should().Be(1.50m);
        result.Data.OTStartAfterMinutes.Should().Be(30);
    }

    [Fact]
    public async Task ProcessOTApprovalAsync_WhenApproved_ShouldSetModifiedByApproverId()
    {
        using var context = CreateDbContext(nameof(ProcessOTApprovalAsync_WhenApproved_ShouldSetModifiedByApproverId));
        var entry = new OTEntry
        {
            Id = 5,
            OrganizationId = 1,
            EmployeeId = 10,
            OTSettingId = 1,
            OTDate = new DateOnly(2026, 8, 1),
            OTHours = 2.5m,
            MultiplierApplied = 1.5m,
            HourlyRate = 100m,
            OTAmount = 375m
        };
        await context.OTEntries.AddAsync(entry);
        await context.SaveChangesAsync();

        var service = new OvertimeService(context);
        var result = await service.ProcessOTApprovalAsync(5, 99, new ApproveOTEntryDto { IsApproved = true });

        result.Success.Should().BeTrue();
        var inDb = await context.OTEntries.FirstOrDefaultAsync(o => o.Id == 5);
        inDb.Should().NotBeNull();
        inDb!.ModifiedBy.Should().Be(99);
    }

    [Fact]
    public async Task ProcessOTApprovalAsync_WhenRejected_ShouldSoftDeleteEntry()
    {
        using var context = CreateDbContext(nameof(ProcessOTApprovalAsync_WhenRejected_ShouldSoftDeleteEntry));
        var entry = new OTEntry
        {
            Id = 6,
            OrganizationId = 1,
            EmployeeId = 10,
            OTSettingId = 1,
            OTDate = new DateOnly(2026, 8, 2),
            OTHours = 1.0m,
            MultiplierApplied = 1.5m,
            HourlyRate = 100m,
            OTAmount = 150m
        };
        await context.OTEntries.AddAsync(entry);
        await context.SaveChangesAsync();

        var service = new OvertimeService(context);
        var result = await service.ProcessOTApprovalAsync(6, 99, new ApproveOTEntryDto { IsApproved = false });

        result.Success.Should().BeTrue();
        var inDb = await context.OTEntries.IgnoreQueryFilters().FirstOrDefaultAsync(o => o.Id == 6);
        inDb.Should().NotBeNull();
        inDb!.IsDeleted.Should().BeTrue();
    }
}
