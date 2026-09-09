using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Interfaces.Payroll;

public interface IPayslipProjectionService
{
    Task<IReadOnlyList<Payslip>> ProjectPayslipsForPeriodAsync(int payrollPeriodId, CancellationToken cancellationToken = default);
    Task<Payslip> ProjectSinglePayslipAsync(int payrollEmployeeId, CancellationToken cancellationToken = default);
}
