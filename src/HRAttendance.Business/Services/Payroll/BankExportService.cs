using System.Text;
using Microsoft.EntityFrameworkCore;
using HRAttendance.Business.Interfaces.Payroll;
using HRAttendance.Data;
using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Services.Payroll;

public class BankExportService : IBankExportService
{
    private readonly ApplicationDbContext _context;

    public BankExportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BankExportResult> GenerateExportBatchAsync(int periodId, string format, int userId, CancellationToken cancellationToken = default)
    {
        var period = await _context.PayrollPeriods
            .Include(p => p.Organization)
            .FirstOrDefaultAsync(p => p.Id == periodId, cancellationToken)
            ?? throw new InvalidOperationException($"Payroll period {periodId} not found.");

        if (period.Status != PayrollPeriodStatus.Locked &&
            period.Status != PayrollPeriodStatus.PaymentProcessing &&
            period.Status != PayrollPeriodStatus.Paid &&
            period.Status != PayrollPeriodStatus.Approved)
        {
            throw new InvalidOperationException($"Payroll period must be Approved, Locked, or in Payment Processing to generate bank export files. Current status: '{period.Status}'.");
        }

        var employees = await _context.PayrollEmployees
            .Include(pe => pe.Employee)
                .ThenInclude(e => e.ContactDetails)
            .Where(pe => pe.PayrollPeriodId == periodId && pe.NetPay > 0)
            .OrderBy(pe => pe.Employee.EmployeeCode)
            .ToListAsync(cancellationToken);

        var totalAmount = employees.Sum(pe => pe.NetPay);
        var totalRecords = employees.Count;

        var count = await _context.BankExportBatches
            .CountAsync(b => b.PayrollPeriodId == periodId, cancellationToken);
        var batchNumber = $"EXP-{period.Year}{period.Month:D2}-{(count + 1):D3}";

        var normalizedFormat = format.ToUpperInvariant();
        var sb = new StringBuilder();
        string fileName;
        const string contentType = "text/csv";

        if (normalizedFormat.Contains("HDFC"))
        {
            fileName = $"{batchNumber}_HDFC_BulkDisbursement.csv";
            sb.AppendLine("Record Type,Beneficiary Code,Beneficiary Name,Account Number,Amount,Payment Type,Remark,Email");
            foreach (var pe in employees)
            {
                var email = pe.Employee.ContactDetails?.WorkEmail ?? pe.Employee.ContactDetails?.OtherEmail ?? "";
                sb.AppendLine($"\"P\",\"{pe.Employee.EmployeeCode}\",\"{pe.Employee.FirstName} {pe.Employee.LastName}\",\"XXXX-XXXX-{(pe.EmployeeId % 10000):D4}\",{pe.NetPay:F2},\"NEFT\",\"Salary {period.Month}/{period.Year}\",\"{email}\"");
            }
        }
        else if (normalizedFormat.Contains("ICICI"))
        {
            fileName = $"{batchNumber}_ICICI_BulkDisbursement.csv";
            sb.AppendLine("Payment Type,Beneficiary Account Number,Amount,Beneficiary Name,IFSC,Sender Name,Remarks");
            foreach (var pe in employees)
            {
                sb.AppendLine($"\"NEFT\",\"XXXX-XXXX-{(pe.EmployeeId % 10000):D4}\",{pe.NetPay:F2},\"{pe.Employee.FirstName} {pe.Employee.LastName}\",\"ICIC0001234\",\"{period.Organization.Name}\",\"Salary {period.Month}/{period.Year}\"");
            }
        }
        else
        {
            // Standard NEFT / RTGS CSV
            fileName = $"{batchNumber}_NEFT_Disbursement.csv";
            sb.AppendLine("Beneficiary Name,Account Number,IFSC Code,Amount,Remarks");
            foreach (var pe in employees)
            {
                sb.AppendLine($"\"{pe.Employee.FirstName} {pe.Employee.LastName}\",\"XXXX-XXXX-{(pe.EmployeeId % 10000):D4}\",\"HDFC0001234\",{pe.NetPay:F2},\"Salary {period.Month}/{period.Year}\"");
            }
        }

        var fileBytes = Encoding.UTF8.GetBytes(sb.ToString());

        var batch = new BankExportBatch
        {
            OrganizationId = period.OrganizationId,
            PayrollPeriodId = period.Id,
            BatchNumber = batchNumber,
            Format = normalizedFormat,
            TotalRecords = totalRecords,
            TotalAmount = totalAmount,
            StoragePath = fileName,
            ExportedAt = DateTimeOffset.UtcNow,
            ExportedBy = userId,
            CreatedBy = userId
        };

        _context.BankExportBatches.Add(batch);
        await _context.SaveChangesAsync(cancellationToken);

        return new BankExportResult(
            BatchId: batch.Id,
            BatchNumber: batchNumber,
            Format: normalizedFormat,
            TotalRecords: totalRecords,
            TotalAmount: totalAmount,
            FileBytes: fileBytes,
            FileName: fileName,
            ContentType: contentType
        );
    }

    public async Task<IReadOnlyList<BankExportBatch>> GetExportBatchesForPeriodAsync(int periodId, CancellationToken cancellationToken = default)
    {
        return await _context.BankExportBatches
            .Where(b => b.PayrollPeriodId == periodId)
            .OrderByDescending(b => b.ExportedAt)
            .ToListAsync(cancellationToken);
    }
}
