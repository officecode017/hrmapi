using FluentAssertions;
using HRAttendance.Business.Services.Payroll;
using HRAttendance.Data.Models.Employee;
using Xunit;

namespace HRAttendance.UnitTests.Payroll;

public class EmploymentPeriodResolverTests
{
    private readonly EmploymentPeriodResolver _resolver = new();

    [Fact]
    public void Resolve_FullMonth_ReturnsAllCalendarDays()
    {
        // Arrange
        var employee = new Employee
        {
            Id = 1,
            ProfessionalDetails = new EmployeeProfessionalDetails
            {
                DateOfJoining = new DateOnly(2025, 1, 1)
            }
        };

        // Act - September 2026 (30 days)
        var window = _resolver.Resolve(employee, 2026, 9);

        // Assert
        window.IsEligible.Should().BeTrue();
        window.StartDate.Should().Be(new DateOnly(2026, 9, 1));
        window.EndDate.Should().Be(new DateOnly(2026, 9, 30));
        window.EligibleDays.Should().Be(30);
        window.TotalDaysInMonth.Should().Be(30);
    }

    [Fact]
    public void Resolve_MidMonthJoining_CalculatesEligibleDaysCorrectly()
    {
        // Arrange - Joins on September 17, 2026
        var employee = new Employee
        {
            Id = 2,
            ProfessionalDetails = new EmployeeProfessionalDetails
            {
                DateOfJoining = new DateOnly(2026, 9, 17)
            }
        };

        // Act
        var window = _resolver.Resolve(employee, 2026, 9);

        // Assert - Active window: Sep 17 to Sep 30 (14 days)
        window.IsEligible.Should().BeTrue();
        window.StartDate.Should().Be(new DateOnly(2026, 9, 17));
        window.EndDate.Should().Be(new DateOnly(2026, 9, 30));
        window.EligibleDays.Should().Be(14);
        window.TotalDaysInMonth.Should().Be(30);
    }

    [Fact]
    public void Resolve_MidMonthExit_CalculatesEligibleDaysCorrectly()
    {
        // Arrange - Exits on September 12, 2026
        var employee = new Employee
        {
            Id = 3,
            ProfessionalDetails = new EmployeeProfessionalDetails
            {
                DateOfJoining = new DateOnly(2024, 1, 1)
            },
            LastWorkingDay = new DateOnly(2026, 9, 12)
        };

        // Act
        var window = _resolver.Resolve(employee, 2026, 9);

        // Assert - Active window: Sep 1 to Sep 12 (12 days)
        window.IsEligible.Should().BeTrue();
        window.StartDate.Should().Be(new DateOnly(2026, 9, 1));
        window.EndDate.Should().Be(new DateOnly(2026, 9, 12));
        window.EligibleDays.Should().Be(12);
        window.TotalDaysInMonth.Should().Be(30);
    }

    [Fact]
    public void Resolve_FutureJoining_ReturnsIneligible()
    {
        // Arrange - Joins next month (October 2026)
        var employee = new Employee
        {
            Id = 4,
            ProfessionalDetails = new EmployeeProfessionalDetails
            {
                DateOfJoining = new DateOnly(2026, 10, 1)
            }
        };

        // Act - Evaluating September 2026
        var window = _resolver.Resolve(employee, 2026, 9);

        // Assert
        window.IsEligible.Should().BeFalse();
        window.EligibleDays.Should().Be(0);
    }
}
