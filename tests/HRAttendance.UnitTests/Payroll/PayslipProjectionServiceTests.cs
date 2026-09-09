using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using HRAttendance.Business.Services.Payroll;
using HRAttendance.Data;
using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Employee;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Payroll;
using Xunit;

namespace HRAttendance.UnitTests.Payroll;

public class PayslipProjectionServiceTests
{
    private ApplicationDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task ProjectSinglePayslipAsync_ShouldCreateExactOneToOneProjection()
    {
        using var context = CreateDbContext(nameof(ProjectSinglePayslipAsync_ShouldCreateExactOneToOneProjection));
        var org = new Organization { Id = 1, Name = "Acme Corp" };
        var emp = new Employee { Id = 10, OrganizationId = 1, EmployeeCode = "EMP010", FirstName = "Alice", LastName = "Smith" };
        var fy = new FinancialYear { Id = 1, OrganizationId = 1, YearCode = "FY2026-27", StartDate = new DateOnly(2026, 4, 1), EndDate = new DateOnly(2027, 3, 31), IsActive = true };
        var period = new PayrollPeriod
        {
            Id = 100,
            OrganizationId = 1,
            FinancialYearId = 1,
            Month = 9,
            Year = 2026,
            StartDate = new DateOnly(2026, 9, 1),
            EndDate = new DateOnly(2026, 9, 30),
            Status = PayrollPeriodStatus.Calculated
        };

        var pe = new PayrollEmployee
        {
            Id = 500,
            PayrollPeriodId = period.Id,
            EmployeeId = emp.Id,
            OrganizationId = 1,
            GrossEarnings = 100000m,
            TotalDeductions = 15000m,
            NetPay = 85000m,
            Period = period,
            Employee = emp
        };

        var item1 = new PayrollItem
        {
            Id = 1,
            PayrollEmployeeId = pe.Id,
            ComponentCode = "BASIC",
            ComponentName = "Basic Salary",
            ComponentType = ComponentType.Earning,
            CalculationOrder = 1,
            FinalAmount = 50000m
        };

        var item2 = new PayrollItem
        {
            Id = 2,
            PayrollEmployeeId = pe.Id,
            ComponentCode = "HRA",
            ComponentName = "House Rent Allowance",
            ComponentType = ComponentType.Earning,
            CalculationOrder = 2,
            FinalAmount = 50000m
        };

        var item3 = new PayrollItem
        {
            Id = 3,
            PayrollEmployeeId = pe.Id,
            ComponentCode = "PF_EE",
            ComponentName = "Provident Fund",
            ComponentType = ComponentType.Deduction,
            CalculationOrder = 3,
            FinalAmount = 15000m
        };

        pe.Items.Add(item1);
        pe.Items.Add(item2);
        pe.Items.Add(item3);

        context.Organizations.Add(org);
        context.Employees.Add(emp);
        context.FinancialYears.Add(fy);
        context.PayrollPeriods.Add(period);
        context.PayrollEmployees.Add(pe);
        await context.SaveChangesAsync();

        var service = new PayslipProjectionService(context);

        // Act
        var payslip = await service.ProjectSinglePayslipAsync(pe.Id);

        // Assert
        payslip.Should().NotBeNull();
        payslip.PayslipNumber.Should().Be("PAY-202609-000010");
        payslip.Status.Should().Be(PayslipStatus.Generated);
        payslip.Items.Should().HaveCount(3);

        var totalEarnings = payslip.Items.Where(i => i.ComponentType == ComponentType.Earning).Sum(i => i.Amount);
        var totalDeductions = payslip.Items.Where(i => i.ComponentType == ComponentType.Deduction).Sum(i => i.Amount);

        totalEarnings.Should().Be(100000m);
        totalDeductions.Should().Be(15000m);
        (totalEarnings - totalDeductions).Should().Be(85000m);
    }

    [Fact]
    public async Task ProjectPayslipsForPeriodAsync_WhenCalledMultipleTimes_ShouldRegenerateCleanlyWithoutDuplicates()
    {
        using var context = CreateDbContext(nameof(ProjectPayslipsForPeriodAsync_WhenCalledMultipleTimes_ShouldRegenerateCleanlyWithoutDuplicates));
        var org = new Organization { Id = 2, Name = "Beta Corp" };
        var emp = new Employee { Id = 20, OrganizationId = 2, EmployeeCode = "EMP020", FirstName = "Bob", LastName = "Jones" };
        var fy = new FinancialYear { Id = 2, OrganizationId = 2, YearCode = "FY2026-27", StartDate = new DateOnly(2026, 4, 1), EndDate = new DateOnly(2027, 3, 31), IsActive = true };
        var period = new PayrollPeriod
        {
            Id = 200,
            OrganizationId = 2,
            FinancialYearId = 2,
            Month = 10,
            Year = 2026,
            StartDate = new DateOnly(2026, 10, 1),
            EndDate = new DateOnly(2026, 10, 31),
            Status = PayrollPeriodStatus.Calculated
        };

        var pe = new PayrollEmployee
        {
            Id = 600,
            PayrollPeriodId = period.Id,
            EmployeeId = emp.Id,
            OrganizationId = 2,
            GrossEarnings = 60000m,
            TotalDeductions = 5000m,
            NetPay = 55000m,
            Period = period,
            Employee = emp
        };

        pe.Items.Add(new PayrollItem { Id = 10, PayrollEmployeeId = pe.Id, ComponentCode = "BASIC", ComponentName = "Basic", ComponentType = ComponentType.Earning, CalculationOrder = 1, FinalAmount = 60000m });
        pe.Items.Add(new PayrollItem { Id = 11, PayrollEmployeeId = pe.Id, ComponentCode = "PT", ComponentName = "Prof Tax", ComponentType = ComponentType.Deduction, CalculationOrder = 2, FinalAmount = 5000m });

        context.Organizations.Add(org);
        context.Employees.Add(emp);
        context.FinancialYears.Add(fy);
        context.PayrollPeriods.Add(period);
        context.PayrollEmployees.Add(pe);
        await context.SaveChangesAsync();

        var service = new PayslipProjectionService(context);

        // Act - First run
        var firstRun = await service.ProjectPayslipsForPeriodAsync(period.Id);
        firstRun.Should().HaveCount(1);
        firstRun[0].Items.Should().HaveCount(2);

        // Act - Second run (regeneration)
        var secondRun = await service.ProjectPayslipsForPeriodAsync(period.Id);

        // Assert
        secondRun.Should().HaveCount(1);
        secondRun[0].Items.Should().HaveCount(2);

        var allPayslipsInDb = await context.Payslips.Where(p => p.PayrollEmployeeId == pe.Id).ToListAsync();
        allPayslipsInDb.Should().HaveCount(1);
    }
}
