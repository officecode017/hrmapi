using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Interfaces.Payroll;

public record OvertimeResult(
    decimal HourlyRate,
    decimal OvertimePay,
    string Formula,
    string Notes
);

public interface IOvertimeCalculationService
{
    OvertimeResult Calculate(
        decimal basicSalary,
        decimal grossSalary,
        decimal approvedOTHours,
        PayrollPolicy policy
    );
}
