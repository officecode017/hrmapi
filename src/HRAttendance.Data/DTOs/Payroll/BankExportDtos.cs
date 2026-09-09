namespace HRAttendance.Data.DTOs.Payroll;

public class GenerateBankExportDto
{
    public int PayrollPeriodId { get; set; }
    public string Format { get; set; } = "CSV_NEFT"; // CSV_NEFT, EXCEL_HDFC, EXCEL_ICICI
}

public class BankExportBatchDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int PayrollPeriodId { get; set; }
    public string PeriodDescription { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public decimal TotalAmount { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public DateTimeOffset ExportedAt { get; set; }
    public int ExportedBy { get; set; }
}
