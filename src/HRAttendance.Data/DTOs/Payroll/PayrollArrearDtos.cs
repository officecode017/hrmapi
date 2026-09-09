using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Data.DTOs.Payroll;

public class CreatePayrollArrearDto
{
    public int EmployeeId { get; set; }
    public int SourcePayrollPeriodId { get; set; }
    public int TargetPayrollPeriodId { get; set; }
    public string ComponentCode { get; set; } = string.Empty;
    public decimal OriginalAmount { get; set; }
    public decimal CorrectAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class PayrollArrearDto
{
    public int Id { get; set; }
    public string ArrearNumber { get; set; } = string.Empty;
    public int OrganizationId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public int SourcePayrollPeriodId { get; set; }
    public string SourcePeriodName { get; set; } = string.Empty;
    public int TargetPayrollPeriodId { get; set; }
    public string TargetPeriodName { get; set; } = string.Empty;
    public string ComponentCode { get; set; } = string.Empty;
    public decimal OriginalAmount { get; set; }
    public decimal CorrectAmount { get; set; }
    public decimal DifferenceAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ArrearStatus Status { get; set; }
}
