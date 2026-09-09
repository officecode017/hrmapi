using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using HRAttendance.Business.Interfaces.Payroll;
using HRAttendance.Business.Services.Payroll;
using HRAttendance.Data;
using HRAttendance.Data.Models.Payroll;
using Xunit;

namespace HRAttendance.UnitTests.Payroll;

public class PayrollAdjustmentServiceTests
{
    private ApplicationDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateAdjustmentAsync_WithPositiveAmount_CreatesPendingAdjustment()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateDbContext(dbName);

        var period = new PayrollPeriod
        {
            Id = 1,
            OrganizationId = 1,
            Month = 9,
            Year = 2026,
            Status = PayrollPeriodStatus.Draft
        };
        context.PayrollPeriods.Add(period);
        await context.SaveChangesAsync();

        var service = new PayrollAdjustmentService(context);

        var dto = new CreateAdjustmentDto(
            OrganizationId: 1,
            EmployeeId: 10,
            PayrollPeriodId: 1,
            Type: AdjustmentType.Bonus,
            Direction: AdjustmentDirection.Earning,
            Amount: 5000m,
            Reason: "Performance bonus"
        );

        var adj = await service.CreateAdjustmentAsync(dto, 99);

        adj.Should().NotBeNull();
        adj.Status.Should().Be(AdjustmentStatus.Pending);
        adj.Amount.Should().Be(5000m);
        adj.AdjustmentNumber.Should().StartWith("ADJ-");
    }

    [Fact]
    public async Task CreateAdjustmentAsync_WithZeroAmount_ThrowsArgumentException()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateDbContext(dbName);

        var service = new PayrollAdjustmentService(context);

        var dto = new CreateAdjustmentDto(
            OrganizationId: 1,
            EmployeeId: 10,
            PayrollPeriodId: 1,
            Type: AdjustmentType.Bonus,
            Direction: AdjustmentDirection.Earning,
            Amount: 0m,
            Reason: "Invalid zero amount"
        );

        var act = () => service.CreateAdjustmentAsync(dto, 99);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*greater than zero*");
    }

    [Fact]
    public async Task ApproveAdjustmentAsync_WhenPending_TransitionsToApproved()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateDbContext(dbName);

        var adj = new PayrollAdjustment
        {
            Id = 1,
            OrganizationId = 1,
            EmployeeId = 10,
            PayrollPeriodId = 1,
            Type = AdjustmentType.Bonus,
            Direction = AdjustmentDirection.Earning,
            Amount = 3000m,
            Reason = "Festival bonus",
            AdjustmentNumber = "ADJ-2026-0001",
            Status = AdjustmentStatus.Pending
        };
        context.PayrollAdjustments.Add(adj);
        await context.SaveChangesAsync();

        var service = new PayrollAdjustmentService(context);
        var result = await service.ApproveAdjustmentAsync(1, 42);

        result.Status.Should().Be(AdjustmentStatus.Approved);
        result.ApprovedBy.Should().Be(42);
        result.ApprovedAt.Should().NotBeNull();
    }
}
