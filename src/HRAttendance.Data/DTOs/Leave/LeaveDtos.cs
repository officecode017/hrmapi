namespace HRAttendance.Data.DTOs.Leave;

public class CreateLeaveApplicationDto
{
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public DateTimeOffset LeaveFrom { get; set; }
    public DateTimeOffset LeaveTo { get; set; }
    public decimal NoOfLeave { get; set; }
    public bool IsHalfDay { get; set; }
    public string? Reason { get; set; }
}

public class LeaveApplicationDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public decimal NoOfLeave { get; set; }
    public DateTimeOffset LeaveFrom { get; set; }
    public DateTimeOffset LeaveTo { get; set; }
    public DateTimeOffset LeaveApplicationDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? ApproverName { get; set; }
}

public class LeaveApprovalDto
{
    public bool IsApproved { get; set; }
    public string? Comments { get; set; }
}

public class LeaveBalanceDto
{
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public decimal Credited { get; set; }
    public decimal BroughtForward { get; set; }
    public decimal Taken { get; set; }
    public decimal Available => (Credited + BroughtForward) - Taken;
}
