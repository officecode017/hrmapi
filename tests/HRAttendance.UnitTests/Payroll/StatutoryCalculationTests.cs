using FluentAssertions;
using HRAttendance.Business.Interfaces.Payroll.Statutory;
using HRAttendance.Business.Services.Payroll.Statutory;
using Xunit;

namespace HRAttendance.UnitTests.Payroll;

public class StatutoryCalculationTests
{
    private readonly PFCalculator _pfCalculator = new();
    private readonly ESICalculator _esiCalculator = new();
    private readonly ProfessionalTaxCalculator _ptCalculator = new();

    [Fact]
    public void PF_EnforcesStatutoryWageCeiling_WhenBasicExceedsLimit()
    {
        // Arrange - Basic ₹25,000 exceeds ₹15,000 ceiling
        var config = new PFConfig(EnforceWageCeiling: true, WageCeiling: 15000m);

        // Act
        var result = _pfCalculator.Calculate(25000m, config);

        // Assert - 12% of 15,000 = 1,800
        result.EmployeePF.Should().Be(1800m);
        // EPS = 8.33% of 15,000 = 1,250
        result.EmployerEPS.Should().Be(1250m);
        // EPF = 1,800 - 1,250 = 550
        result.EmployerEPF.Should().Be(550m);
        result.TotalEmployerPF.Should().Be(1800m);
    }

    [Fact]
    public void ESI_AppliesOnlyWhenGrossBelowOrAtThreshold()
    {
        var config = new ESIConfig(WageThreshold: 21000m, EmployeeRate: 0.0075m, EmployerRate: 0.0325m);

        // Case 1: Gross ₹20,000 <= ₹21,000 (Applicable)
        var resultApplicable = _esiCalculator.Calculate(20000m, config);
        resultApplicable.IsApplicable.Should().BeTrue();
        resultApplicable.EmployeeESI.Should().Be(150m); // 20,000 * 0.75% = 150
        resultApplicable.EmployerESI.Should().Be(650m); // 20,000 * 3.25% = 650

        // Case 2: Gross ₹22,000 > ₹21,000 (Exempt)
        var resultExempt = _esiCalculator.Calculate(22000m, config);
        resultExempt.IsApplicable.Should().BeFalse();
        resultExempt.EmployeeESI.Should().Be(0m);
        resultExempt.EmployerESI.Should().Be(0m);
    }

    [Fact]
    public void PT_AppliesFebruarySpecialRate_WhenInFebruary()
    {
        var brackets = new List<PTBracket>
        {
            new(0m, 7500m, 0m),
            new(7501m, 10000m, 175m),
            new(10001m, decimal.MaxValue, 200m, FebruaryTax: 300m)
        };
        var config = new PTConfig("MH", brackets);

        // Standard Month (September = 9)
        var sepResult = _ptCalculator.Calculate(25000m, 9, config);
        sepResult.PTAmount.Should().Be(200m);

        // February (Month = 2)
        var febResult = _ptCalculator.Calculate(25000m, 2, config);
        febResult.PTAmount.Should().Be(300m);
    }
}
