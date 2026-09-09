using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Interfaces.Payroll;

public record LOPResult(
    decimal DailyRate,
    decimal LOPDeduction,
    string Formula,
    string Notes
);

public interface ILOPCalculationService
{
    LOPResult Calculate(
        decimal monthlyGross,
        decimal lopDays,
        int calendarDaysInMonth,
        decimal workingDaysInMonth,
        PayrollPolicy policy
    );
}
