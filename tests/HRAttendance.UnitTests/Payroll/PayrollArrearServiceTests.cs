using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using HRAttendance.Business.Interfaces.Payroll;
using HRAttendance.Business.Services.Payroll;
using HRAttendance.Data;
using HRAttendance.Data.Models.Payroll;
using Xunit;

namespace HRAttendance.UnitTests.Payroll;

public class PayrollArrearServiceTests
{
    private ApplicationDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateArrearAsync_ComputesDifferenceAccurately()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateDbContext(dbName);

        var targetPeriod = new PayrollPeriod
        {
            Id = 2,
            OrganizationId = 1,
            Month = 10,
            Year = 2026,
            Status = PayrollPeriodStatus.Draft
        };
        context.PayrollPeriods.Add(targetPeriod);
        await context.SaveChangesAsync();

        var service = new PayrollArrearService(context);

        var dto = new CreateArrearDto(
            OrganizationId: 1,
            EmployeeId: 10,
            SourcePayrollPeriodId: 1,
            TargetPayrollPeriodId: 2,
            ComponentCode: "BASIC",
            OriginalAmount: 20000m,
            CorrectAmount: 25000m,
            Reason: "Retroactive pay hike"
        );

        var arrear = await service.CreateArrearAsync(dto, 99);

        arrear.Should().NotBeNull();
        arrear.DifferenceAmount.Should().Be(5000m);
        arrear.Status.Should().Be(ArrearStatus.Pending);
        arrear.ArrearNumber.Should().StartWith("ARR-");
    }

    [Fact]
    public async Task ApplyArrearAsync_TransitionsToApplied()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateDbContext(dbName);

        var arrear = new PayrollArrear
        {
            Id = 1,
            OrganizationId = 1,
            EmployeeId = 10,
            SourcePayrollPeriodId = 1,
            ComponentCode = "BASIC",
            OriginalAmount = 20000m,
            CorrectAmount = 25000m,
            DifferenceAmount = 5000m,
            Reason = "Pay revision",
            ArrearNumber = "ARR-2026-0001",
            Status = ArrearStatus.Pending
        };
        context.PayrollArrears.Add(arrear);
        await context.SaveChangesAsync();

        var service = new PayrollArrearService(context);
        var result = await service.ApplyArrearAsync(1, 2);

        result.Status.Should().Be(ArrearStatus.Applied);
        result.TargetPayrollPeriodId.Should().Be(2);
    }
}
