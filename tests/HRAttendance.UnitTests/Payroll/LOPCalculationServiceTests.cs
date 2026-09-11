using FluentAssertions;
using HRAttendance.Business.Services.Payroll;
using HRAttendance.Data.Models.Payroll;
using Xunit;

namespace HRAttendance.UnitTests.Payroll;

public class LOPCalculationServiceTests
{
    private readonly LOPCalculationService _service = new();

    [Fact]
    public void Calculate_CalendarDaysBasis_ComputesExactDailyRate()
    {
        // Arrange - Monthly Gross ₹30,000, 30 calendar days, 2 days LOP
        var policy = new PayrollPolicy { LOPBasis = LOPCalculationBasis.CalendarDays, RoundingRule = RoundingRule.TwoDecimals };

        // Act
        var result = _service.Calculate(30000m, 2m, 30, 22m, policy);

        // Assert
        result.DailyRate.Should().Be(1000m);
        result.LOPDeduction.Should().Be(2000m);
    }

    [Fact]
    public void Calculate_Fixed26DaysBasis_ComputesAccordingToFixedDivisor()
    {
        // Arrange - Monthly Gross ₹52,000, Fixed 26 days divisor, 1.5 days LOP
        var policy = new PayrollPolicy
        {
            LOPBasis = LOPCalculationBasis.FixedDays,
            FixedLOPDays = 26,
            RoundingRule = RoundingRule.TwoDecimals
        };

        // Act
        var result = _service.Calculate(52000m, 1.5m, 30, 22m, policy);

        // Assert - Daily rate = 52,000 / 26 = 2,000. 1.5 days = 3,000.
        result.DailyRate.Should().Be(2000m);
        result.LOPDeduction.Should().Be(3000m);
    }

    [Fact]
    public void Calculate_ZeroLOPDays_ReturnsZeroDeduction()
    {
        // Arrange
        var policy = new PayrollPolicy { LOPBasis = LOPCalculationBasis.CalendarDays };

        // Act
        var result = _service.Calculate(50000m, 0m, 30, 22m, policy);

        // Assert
        result.LOPDeduction.Should().Be(0m);
    }
}
