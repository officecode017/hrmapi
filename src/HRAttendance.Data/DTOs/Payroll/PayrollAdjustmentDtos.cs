using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Data.DTOs.Payroll;

public class CreatePayrollAdjustmentDto
{
    public int EmployeeId { get; set; }
    public int PayrollPeriodId { get; set; }
    public AdjustmentType Type { get; set; }
    public AdjustmentDirection Direction { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class RejectAdjustmentDto
{
    public string Reason { get; set; } = string.Empty;
}

public class PayrollAdjustmentDto
{
    public int Id { get; set; }
    public string AdjustmentNumber { get; set; } = string.Empty;
    public int OrganizationId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public int PayrollPeriodId { get; set; }
    public AdjustmentType Type { get; set; }
    public AdjustmentDirection Direction { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public AdjustmentStatus Status { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public int? RejectedBy { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
}
