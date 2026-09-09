using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Interfaces.Payroll;

public record BankExportResult(
    int BatchId,
    string BatchNumber,
    string Format,
    int TotalRecords,
    decimal TotalAmount,
    byte[] FileBytes,
    string FileName,
    string ContentType
);

public interface IBankExportService
{
    Task<BankExportResult> GenerateExportBatchAsync(int periodId, string format, int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BankExportBatch>> GetExportBatchesForPeriodAsync(int periodId, CancellationToken cancellationToken = default);
}
