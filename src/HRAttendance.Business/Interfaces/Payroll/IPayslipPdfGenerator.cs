namespace HRAttendance.Business.Interfaces.Payroll;

public interface IPayslipPdfGenerator
{
    Task<byte[]> GeneratePayslipPdfAsync(int payslipId, CancellationToken cancellationToken = default);
}
