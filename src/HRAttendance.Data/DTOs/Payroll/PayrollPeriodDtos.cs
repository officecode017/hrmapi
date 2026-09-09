using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Data.DTOs.Payroll;

public class CreatePayrollPeriodDto
{
    public int FinancialYearId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public PayrollRunType RunType { get; set; } = PayrollRunType.Regular;
    public int SequenceNumber { get; set; } = 1;
    public string Description { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}

public class PayrollPeriodDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int FinancialYearId { get; set; }
    public string YearCode { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public PayrollRunType RunType { get; set; }
    public int SequenceNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public PayrollPeriodStatus Status { get; set; }
    public decimal TotalGrossPay { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalEmployerContributions { get; set; }
    public decimal TotalNetPay { get; set; }
    public int TotalEmployeesProcessed { get; set; }
    public int TotalExceptionsCount { get; set; }
    public DateTimeOffset? CalculatedAt { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public int? LockedBy { get; set; }
    public DateTimeOffset? LockedAt { get; set; }
}

public class PayrollPeriodSummaryDto
{
    public int PeriodId { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public PayrollPeriodStatus Status { get; set; }
    public PayrollRunType RunType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal TotalGrossPay { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalEmployerContributions { get; set; }
    public decimal TotalNetPay { get; set; }
    public int TotalEmployees { get; set; }
    public int PendingEmployees { get; set; }
    public int ProcessedEmployees { get; set; }
    public int ErrorEmployees { get; set; }
    public int CriticalExceptionsCount { get; set; }
    public int WarningExceptionsCount { get; set; }
    public List<DepartmentPayrollSummaryDto> DepartmentBreakdown { get; set; } = new();
}

public class DepartmentPayrollSummaryDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal TotalGrossPay { get; set; }
    public decimal TotalNetPay { get; set; }
}

public class PeriodQueryParameters : PagedRequestDto
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    public PayrollPeriodStatus? Status { get; set; }
    public PayrollRunType? RunType { get; set; }
}
