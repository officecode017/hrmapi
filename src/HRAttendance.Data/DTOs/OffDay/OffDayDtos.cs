namespace HRAttendance.Data.DTOs.OffDay;

public class OffDayDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int AcademicYearId { get; set; }
    public int LocationId { get; set; }
    public string? LocationName { get; set; }
    public int RoleId { get; set; }
    public string? RoleName { get; set; }
    public string OffDayName { get; set; } = string.Empty;
    public int WorkDayType { get; set; }
    public bool Week1 { get; set; }
    public bool Week2 { get; set; }
    public bool Week3 { get; set; }
    public bool Week4 { get; set; }
    public bool Week5 { get; set; }
    public bool Week6 { get; set; }
}

public class CreateOffDayDto
{
    public int OrganizationId { get; set; } = 1;
    public int AcademicYearId { get; set; }
    public int LocationId { get; set; }
    public int RoleId { get; set; }
    public string OffDayName { get; set; } = string.Empty;
    public int WorkDayType { get; set; } = 1;
    public bool Week1 { get; set; } = true;
    public bool Week2 { get; set; } = true;
    public bool Week3 { get; set; } = true;
    public bool Week4 { get; set; } = true;
    public bool Week5 { get; set; } = true;
    public bool Week6 { get; set; } = true;
}

public class UpdateOffDayDto
{
    public int AcademicYearId { get; set; }
    public int LocationId { get; set; }
    public int RoleId { get; set; }
    public string OffDayName { get; set; } = string.Empty;
    public int WorkDayType { get; set; }
    public bool Week1 { get; set; }
    public bool Week2 { get; set; }
    public bool Week3 { get; set; }
    public bool Week4 { get; set; }
    public bool Week5 { get; set; }
    public bool Week6 { get; set; }
}

public class SaveOffDayMatrixDto
{
    public int OrganizationId { get; set; } = 1;
    public int AcademicYearId { get; set; } = 1;
    public int LocationId { get; set; }
    public int RoleId { get; set; }
    public List<OffDayMatrixItemDto> Days { get; set; } = new();
}

public class OffDayMatrixItemDto
{
    public string OffDayName { get; set; } = string.Empty; // e.g. "Monday", "Tuesday", etc.
    public int WorkDayType { get; set; } // 0: Working, 1: Full Day Off, 2: Half Day Off
    public bool Week1 { get; set; }
    public bool Week2 { get; set; }
    public bool Week3 { get; set; }
    public bool Week4 { get; set; }
    public bool Week5 { get; set; }
    public bool Week6 { get; set; }
}

