using HRAttendance.Data.Models.Employee;

namespace HRAttendance.Business.Interfaces.Payroll;

public record EmploymentWindow(
    bool IsEligible,
    DateOnly StartDate,
    DateOnly EndDate,
    int EligibleDays,
    int TotalDaysInMonth,
    string? Notes
);

public interface IEmploymentPeriodResolver
{
    EmploymentWindow Resolve(Employee employee, int year, int month);
}
