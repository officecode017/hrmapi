using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using HRAttendance.Business.Services.Payroll;
using HRAttendance.Data;
using HRAttendance.Data.Models.Employee;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Payroll;
using Xunit;

namespace HRAttendance.UnitTests.Payroll;

public class BankExportServiceTests
{
    private ApplicationDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GenerateExportBatchAsync_WhenPeriodIsApproved_GeneratesCsvSuccessfully()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateDbContext(dbName);

        var org = new Organization { Id = 1, Name = "Acme Corp" };
        context.Organizations.Add(org);

        var period = new PayrollPeriod
        {
            Id = 1,
            OrganizationId = 1,
            Month = 9,
            Year = 2026,
            Status = PayrollPeriodStatus.Approved,
            Description = "September 2026"
        };
        context.PayrollPeriods.Add(period);

        var employee = new Employee
        {
            Id = 1,
            OrganizationId = 1,
            EmployeeCode = "EMP001",
            FirstName = "Alice",
            LastName = "Smith"
        };
        context.Employees.Add(employee);

        var pe = new PayrollEmployee
        {
            Id = 1,
            OrganizationId = 1,
            PayrollPeriodId = 1,
            EmployeeId = 1,
            NetPay = 45000m,
            GrossEarnings = 50000m,
            TotalDeductions = 5000m
        };
        context.PayrollEmployees.Add(pe);
        await context.SaveChangesAsync();

        var service = new BankExportService(context);
        var result = await service.GenerateExportBatchAsync(1, "CSV", 99);

        result.Should().NotBeNull();
        result.TotalRecords.Should().Be(1);
        result.TotalAmount.Should().Be(45000m);
        result.FileBytes.Should().NotBeEmpty();
        result.FileName.Should().EndWith(".csv");

        var contentString = System.Text.Encoding.UTF8.GetString(result.FileBytes);
        contentString.Should().Contain("Alice Smith");
        contentString.Should().Contain("45000.00");
    }

    [Fact]
    public async Task GenerateExportBatchAsync_WhenPeriodIsDraft_ThrowsInvalidOperationException()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateDbContext(dbName);

        var org = new Organization { Id = 1, Name = "Acme Corp" };
        context.Organizations.Add(org);

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

        var service = new BankExportService(context);

        var act = () => service.GenerateExportBatchAsync(1, "CSV", 99);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*must be Approved, Locked, or in Payment Processing*");
    }
}
