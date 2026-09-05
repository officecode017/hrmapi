namespace HRAttendance.Data.DTOs.Leave;

public class LeaveSettingDto
{
    public int Id { get; set; }
    public bool IsPaid { get; set; }
    public decimal Leaves { get; set; }
    public bool CanTakeHalfDay { get; set; }
    public decimal CarryForwardLeaveCount { get; set; }
}

public class LeaveTypeDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public LeaveSettingDto? Setting { get; set; }
}

public class CreateLeaveTypeDto
{
    public int OrganizationId { get; set; } = 1;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Associated LeaveSetting
    public bool IsPaid { get; set; } = true;
    public decimal Leaves { get; set; } = 12;
    public bool CanTakeHalfDay { get; set; } = true;
    public decimal CarryForwardLeaveCount { get; set; } = 0;
}

public class UpdateLeaveTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    // Associated LeaveSetting
    public bool IsPaid { get; set; }
    public decimal Leaves { get; set; }
    public bool CanTakeHalfDay { get; set; }
    public decimal CarryForwardLeaveCount { get; set; }
}

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

public class AdjustLeaveBalanceDto
{
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public int AcademicYearId { get; set; }
    public decimal LeaveCredited { get; set; }
    public decimal LeaveBroughtForward { get; set; }
    public decimal LeavesTaken { get; set; }
    public string? Remark { get; set; }
}

public class EmployeeLeaveBalanceDetailDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? DesignationName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public int AcademicYearId { get; set; }
    public DateOnly AcademicYearStartDate { get; set; }
    public DateOnly AcademicYearEndDate { get; set; }
    public string AcademicYearName => $"{AcademicYearStartDate:yyyy} - {AcademicYearEndDate:yyyy}";
    public decimal LeaveCredited { get; set; }
    public decimal LeaveBroughtForward { get; set; }
    public decimal LeavesTaken { get; set; }
    public decimal AvailableBalance => (LeaveCredited + LeaveBroughtForward) - LeavesTaken;
}


public class BulkAllocateLeaveBalanceDto
{
    public int OrganizationId { get; set; } = 1;
    public int AcademicYearId { get; set; }
    public int? DepartmentId { get; set; }
    public int? LeaveTypeId { get; set; }
    public bool OverwriteExisting { get; set; } = false;
}

