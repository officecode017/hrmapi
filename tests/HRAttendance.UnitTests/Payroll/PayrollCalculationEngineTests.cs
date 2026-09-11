using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using HRAttendance.Business.Interfaces.Payroll;
using HRAttendance.Business.Interfaces.Payroll.Statutory;
using HRAttendance.Business.Services.Payroll;
using HRAttendance.Data;
using HRAttendance.Data.Models.Attendance;
using HRAttendance.Data.Models.Employee;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Payroll;
using Xunit;

namespace HRAttendance.UnitTests.Payroll;

public class PayrollCalculationEngineTests
{
    private ApplicationDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CalculateEmployeePayrollAsync_WithValidStructure_CalculatesAccurately()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateDbContext(dbName);

        // Seed Organization & Financial Year
        var org = new Organization { Id = 1, Name = "Acme Corp" };
        context.Organizations.Add(org);

        var fy = new FinancialYear
        {
            Id = 1,
            OrganizationId = 1,
            YearCode = "2026-27",
            StartDate = new DateOnly(2026, 4, 1),
            EndDate = new DateOnly(2027, 3, 31),
            IsActive = true
        };
        context.FinancialYears.Add(fy);

        var profDetails = new EmployeeProfessionalDetails
        {
            OrganizationId = 1,
            EmployeeId = 1,
            DateOfJoining = new DateOnly(2025, 1, 1)
        };

        // Seed Employee
        var employee = new Employee
        {
            Id = 1,
            OrganizationId = 1,
            EmployeeCode = "EMP001",
            FirstName = "John",
            LastName = "Doe",
            ProfessionalDetails = profDetails
        };
        context.Employees.Add(employee);

        // Seed Policy
        var policy = new PayrollPolicy
        {
            OrganizationId = 1,
            ProrationBasis = SalaryProrationBasis.ActualCalendarDays,
            LOPBasis = LOPCalculationBasis.CalendarDays,
            OTBasis = OvertimeBasis.BasicSalary,
            OTMultiplier = 1.5m,
            StandardMonthlyWorkingHours = 160m
        };
        context.PayrollPolicies.Add(policy);

        // Seed Salary Components
        var basicComp = new SalaryComponent
        {
            Id = 1,
            OrganizationId = 1,
            Code = "BASIC",
            Name = "Basic Salary",
            Type = ComponentType.Earning,
            CalculationType = ComponentCalculationType.FixedAmount,
            IsActive = true
        };
        var hraComp = new SalaryComponent
        {
            Id = 2,
            OrganizationId = 1,
            Code = "HRA",
            Name = "House Rent Allowance",
            Type = ComponentType.Earning,
            CalculationType = ComponentCalculationType.FixedAmount,
            IsActive = true
        };
        context.SalaryComponents.AddRange(basicComp, hraComp);

        // Seed Salary Structure
        var structure = new EmployeeSalaryStructure
        {
            Id = 1,
            OrganizationId = 1,
            EmployeeId = 1,
            Version = 1,
            EffectiveFrom = new DateOnly(2025, 1, 1),
            MonthlyGrossSalary = 37500m,
            AnnualCTC = 450000m,
            IsActive = true
        };
        employee.SalaryStructures.Add(structure);
        context.EmployeeSalaryStructures.Add(structure);

        // Seed Items
        var item1 = new EmployeeSalaryStructureItem
        {
            Id = 1,
            EmployeeSalaryStructureId = 1,
            SalaryComponentId = 1,
            Component = basicComp,
            MonthlyAmount = 25000m,
            AnnualAmount = 300000m
        };
        var item2 = new EmployeeSalaryStructureItem
        {
            Id = 2,
            EmployeeSalaryStructureId = 1,
            SalaryComponentId = 2,
            Component = hraComp,
            MonthlyAmount = 12500m,
            AnnualAmount = 150000m
        };
        structure.Items.Add(item1);
        structure.Items.Add(item2);
        context.EmployeeSalaryStructureItems.AddRange(item1, item2);

        // Seed Period (September 2026 = 30 days)
        var period = new PayrollPeriod
        {
            Id = 1,
            OrganizationId = 1,
            FinancialYearId = 1,
            Month = 9,
            Year = 2026,
            StartDate = new DateOnly(2026, 9, 1),
            EndDate = new DateOnly(2026, 9, 30),
            Status = PayrollPeriodStatus.Draft,
            Description = "September 2026"
        };
        context.PayrollPeriods.Add(period);

        await context.SaveChangesAsync();

        // Setup Engine
        var resolver = new EmploymentPeriodResolver();
        var proration = new MidMonthProrationService();
        var lop = new LOPCalculationService();
        var overtime = new OvertimeCalculationService();
        
        var mockStatutory = new Mock<IStatutoryCalculationService>();
        mockStatutory.Setup(s => s.CalculateStatutoryAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<decimal>(),
                It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(new StatutoryDeductionsResult(
                null,
                null,
                null,
                null,
                2000m,
                1800m,
                new List<PayrollItem>()
            ));

        var engine = new PayrollCalculationEngine(context, resolver, proration, lop, overtime, mockStatutory.Object);

        // Act
        var result = await engine.CalculateEmployeePayrollAsync(1, 1);

        // Assert
        result.Should().NotBeNull();
        result.EmployeeId.Should().Be(1);
        result.GrossEarnings.Should().Be(37500m); // 25000 + 12500
        result.TotalDeductions.Should().Be(2000m);
        result.NetPay.Should().Be(35500m); // 37500 - 2000
        result.Status.Should().Be(PayrollEmployeeStatus.Calculated);
        result.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CalculatePeriodBatchAsync_WhenPeriodIsLocked_ThrowsInvalidOperationException()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateDbContext(dbName);

        var org = new Organization { Id = 1, Name = "Acme Corp" };
        context.Organizations.Add(org);

        var fy = new FinancialYear
        {
            Id = 1,
            OrganizationId = 1,
            YearCode = "2026-27",
            StartDate = new DateOnly(2026, 4, 1),
            EndDate = new DateOnly(2027, 3, 31),
            IsActive = true
        };
        context.FinancialYears.Add(fy);

        var period = new PayrollPeriod
        {
            Id = 2,
            OrganizationId = 1,
            FinancialYearId = 1,
            Month = 9,
            Year = 2026,
            StartDate = new DateOnly(2026, 9, 1),
            EndDate = new DateOnly(2026, 9, 30),
            Status = PayrollPeriodStatus.Locked
        };
        context.PayrollPeriods.Add(period);
        await context.SaveChangesAsync();

        var resolver = new EmploymentPeriodResolver();
        var proration = new MidMonthProrationService();
        var lop = new LOPCalculationService();
        var overtime = new OvertimeCalculationService();
        var mockStatutory = new Mock<IStatutoryCalculationService>();

        var engine = new PayrollCalculationEngine(context, resolver, proration, lop, overtime, mockStatutory.Object);

        var act = () => engine.CalculatePeriodBatchAsync(2);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Locked*");
    }
}
