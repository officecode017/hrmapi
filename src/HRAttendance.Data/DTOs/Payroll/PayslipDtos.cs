using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Data.DTOs.Payroll;

public class PayslipDto
{
    public int Id { get; set; }
    public int PayrollEmployeeId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string DesignationName { get; set; } = string.Empty;
    public string PayslipNumber { get; set; } = string.Empty;
    public PayslipStatus Status { get; set; }
    public DateTimeOffset GeneratedAt { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal GrossEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetPay { get; set; }
    public List<PayslipItemPresentationDto> Earnings { get; set; } = new();
    public List<PayslipItemPresentationDto> Deductions { get; set; } = new();
    public List<PayslipItemPresentationDto> EmployerContributions { get; set; } = new();
}

public class PayslipItemPresentationDto
{
    public int Id { get; set; }
    public string ComponentCode { get; set; } = string.Empty;
    public string ComponentName { get; set; } = string.Empty;
    public ComponentType ComponentType { get; set; }
    public decimal Amount { get; set; }
    public int DisplayOrder { get; set; }
}

public class PayslipAccessLogDto
{
    public int Id { get; set; }
    public int PayslipId { get; set; }
    public int AccessedBy { get; set; }
    public string AccessedByName { get; set; } = string.Empty;
    public DateTimeOffset AccessedAt { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public PayslipActionType ActionType { get; set; }
}

public class MyPayslipSummaryDto
{
    public int Id { get; set; }
    public string PayslipNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public string PeriodDescription { get; set; } = string.Empty;
    public decimal GrossEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetPay { get; set; }
    public PayslipStatus Status { get; set; }
    public DateTimeOffset GeneratedAt { get; set; }
}
