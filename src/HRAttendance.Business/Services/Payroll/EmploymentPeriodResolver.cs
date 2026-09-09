using HRAttendance.Business.Interfaces.Payroll;
using HRAttendance.Data.Models.Employee;

namespace HRAttendance.Business.Services.Payroll;

public class EmploymentPeriodResolver : IEmploymentPeriodResolver
{
    public EmploymentWindow Resolve(Employee employee, int year, int month)
    {
        var totalDaysInMonth = DateTime.DaysInMonth(year, month);
        var periodStart = new DateOnly(year, month, 1);
        var periodEnd = new DateOnly(year, month, totalDaysInMonth);

        var joiningDate = employee.ProfessionalDetails?.DateOfJoining;
        var exitDate = employee.LastWorkingDay ?? employee.ResignationDate;

        // 1. Employee joined after this period
        if (joiningDate.HasValue && joiningDate.Value > periodEnd)
        {
            return new EmploymentWindow(
                IsEligible: false,
                StartDate: periodStart,
                EndDate: periodEnd,
                EligibleDays: 0,
                TotalDaysInMonth: totalDaysInMonth,
                Notes: $"Employee joined on {joiningDate.Value:yyyy-MM-dd}, which is after period end {periodEnd:yyyy-MM-dd}."
            );
        }

        // 2. Employee exited before this period
        if (exitDate.HasValue && exitDate.Value < periodStart)
        {
            return new EmploymentWindow(
                IsEligible: false,
                StartDate: periodStart,
                EndDate: periodEnd,
                EligibleDays: 0,
                TotalDaysInMonth: totalDaysInMonth,
                Notes: $"Employee exited on {exitDate.Value:yyyy-MM-dd}, which is before period start {periodStart:yyyy-MM-dd}."
            );
        }

        // 3. Compute active boundaries within period
        var activeStart = joiningDate.HasValue && joiningDate.Value > periodStart ? joiningDate.Value : periodStart;
        var activeEnd = exitDate.HasValue && exitDate.Value < periodEnd ? exitDate.Value : periodEnd;

        if (activeStart > activeEnd)
        {
            return new EmploymentWindow(
                IsEligible: false,
                StartDate: activeStart,
                EndDate: activeEnd,
                EligibleDays: 0,
                TotalDaysInMonth: totalDaysInMonth,
                Notes: "Active employment start date exceeds exit date."
            );
        }

        var eligibleDays = (activeEnd.DayNumber - activeStart.DayNumber) + 1;
        var notes = eligibleDays == totalDaysInMonth
            ? "Full month active employment."
            : $"Partial month active employment from {activeStart:yyyy-MM-dd} to {activeEnd:yyyy-MM-dd} ({eligibleDays} of {totalDaysInMonth} days).";

        return new EmploymentWindow(
            IsEligible: true,
            StartDate: activeStart,
            EndDate: activeEnd,
            EligibleDays: eligibleDays,
            TotalDaysInMonth: totalDaysInMonth,
            Notes: notes
        );
    }
}
