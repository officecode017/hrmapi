namespace HRAttendance.Data.DTOs.Shift;

public class ShiftDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int LocationId { get; set; }
    public string? LocationName { get; set; }
    public string Name { get; set; } = string.Empty;
    public TimeOnly InTime { get; set; }
    public TimeOnly OutTime { get; set; }
    public bool IsOvernight { get; set; }
    public int GraceMinutes { get; set; }
    public int BreakMinutes { get; set; }
    public TimeOnly? BreakStartTime { get; set; }
    public TimeOnly? BreakEndTime { get; set; }
}

public class CreateShiftDto
{
    public int OrganizationId { get; set; } = 1;
    public int LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public TimeOnly InTime { get; set; }
    public TimeOnly OutTime { get; set; }
    public bool IsOvernight { get; set; }
    public int GraceMinutes { get; set; } = 15;
    public int BreakMinutes { get; set; } = 60;
    public TimeOnly? BreakStartTime { get; set; }
    public TimeOnly? BreakEndTime { get; set; }
}

public class UpdateShiftDto
{
    public int LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public TimeOnly InTime { get; set; }
    public TimeOnly OutTime { get; set; }
    public bool IsOvernight { get; set; }
    public int GraceMinutes { get; set; }
    public int BreakMinutes { get; set; }
    public TimeOnly? BreakStartTime { get; set; }
    public TimeOnly? BreakEndTime { get; set; }
}

public class AssignShiftDto
{
    public int EmployeeId { get; set; }
    public int ShiftId { get; set; }
}

public class BulkAssignShiftDto
{
    public List<int> EmployeeIds { get; set; } = new();
    public int ShiftId { get; set; }
}
