using FluentAssertions;
using HRAttendance.Business.Interfaces.Payroll;
using HRAttendance.Business.Services.Payroll;
using HRAttendance.Data.Models.Payroll;
using Xunit;

namespace HRAttendance.UnitTests.Payroll;

public class MidMonthProrationServiceTests
{
    private readonly MidMonthProrationService _service = new();
    private readonly PayrollPolicy _policy = new() { ProrationBasis = SalaryProrationBasis.ActualCalendarDays };

    [Fact]
    public void CalculateSlices_SingleStructure_ReturnsFullGross()
    {
        // Arrange
        var window = new EmploymentWindow(true, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30), 30, 30, null);
        var structure = new EmployeeSalaryStructure
        {
            Id = 10,
            Version = 1,
            EffectiveFrom = new DateOnly(2026, 1, 1),
            MonthlyGrossSalary = 50000m
        };

        // Act
        var result = _service.CalculateSlices(window, new[] { structure }, _policy);

        // Assert
        result.Slices.Should().HaveCount(1);
        result.Slices[0].MonthlyGrossInSlice.Should().Be(50000m);
        result.Slices[0].ProratedGrossInSlice.Should().Be(50000m);
        result.ProratedGross.Should().Be(50000m);
    }

    [Fact]
    public void CalculateSlices_MidMonthRevision_SplitsSegmentsAccurately()
    {
        // Arrange - September 2026 (30 days)
        // Revision on Sep 16: V1 (Sep 1-15) = 50,000/mo, V2 (Sep 16-30) = 60,000/mo
        var window = new EmploymentWindow(true, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30), 30, 30, null);

        var v1 = new EmployeeSalaryStructure
        {
            Id = 1,
            Version = 1,
            EffectiveFrom = new DateOnly(2026, 1, 1),
            EffectiveTo = new DateOnly(2026, 9, 15),
            MonthlyGrossSalary = 50000m
        };

        var v2 = new EmployeeSalaryStructure
        {
            Id = 2,
            Version = 2,
            EffectiveFrom = new DateOnly(2026, 9, 16),
            EffectiveTo = null,
            MonthlyGrossSalary = 60000m
        };

        // Act
        var result = _service.CalculateSlices(window, new[] { v1, v2 }, _policy);

        // Assert
        result.Slices.Should().HaveCount(2);

        // Slice 1: Sep 1 to Sep 15 (15 days) -> 50,000 * 15 / 30 = 25,000
        var slice1 = result.Slices[0];
        slice1.SalaryStructureVersion.Should().Be(1);
        slice1.SliceStartDate.Should().Be(new DateOnly(2026, 9, 1));
        slice1.SliceEndDate.Should().Be(new DateOnly(2026, 9, 15));
        slice1.TotalCalendarDaysInSlice.Should().Be(15);
        slice1.ProratedGrossInSlice.Should().Be(25000m);

        // Slice 2: Sep 16 to Sep 30 (15 days) -> 60,000 * 15 / 30 = 30,000
        var slice2 = result.Slices[1];
        slice2.SalaryStructureVersion.Should().Be(2);
        slice2.SliceStartDate.Should().Be(new DateOnly(2026, 9, 16));
        slice2.SliceEndDate.Should().Be(new DateOnly(2026, 9, 30));
        slice2.TotalCalendarDaysInSlice.Should().Be(15);
        slice2.ProratedGrossInSlice.Should().Be(30000m);

        // Total Prorated Gross = 25,000 + 30,000 = 55,000
        result.ProratedGross.Should().Be(55000m);
    }
}
