using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Data.DTOs.Payroll;

public class CreateSalaryStructureDto
{
    public int EmployeeId { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public decimal MonthlyGrossSalary { get; set; }
    public decimal AnnualCTC { get; set; }
    public string? RevisionReason { get; set; }
    public List<CreateSalaryStructureItemDto> Items { get; set; } = new();
}

public class CreateSalaryStructureItemDto
{
    public int SalaryComponentId { get; set; }
    public decimal MonthlyAmount { get; set; }
    public decimal AnnualAmount { get; set; }
    public decimal PercentageRate { get; set; }
}

public class ReviseSalaryStructureDto
{
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public decimal MonthlyGrossSalary { get; set; }
    public decimal AnnualCTC { get; set; }
    public string RevisionReason { get; set; } = string.Empty;
    public List<CreateSalaryStructureItemDto> Items { get; set; } = new();
}

public class SalaryStructureDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public int Version { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public decimal MonthlyGrossSalary { get; set; }
    public decimal AnnualCTC { get; set; }
    public string? RevisionReason { get; set; }
    public bool IsActive { get; set; }
    public List<SalaryStructureItemDto> Items { get; set; } = new();
}

public class SalaryStructureItemDto
{
    public int Id { get; set; }
    public int SalaryComponentId { get; set; }
    public string ComponentCode { get; set; } = string.Empty;
    public string ComponentName { get; set; } = string.Empty;
    public ComponentType ComponentType { get; set; }
    public decimal MonthlyAmount { get; set; }
    public decimal AnnualAmount { get; set; }
    public decimal PercentageRate { get; set; }
}

public class SalaryHistoryTimelineDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public List<SalaryHistoryItemDto> History { get; set; } = new();
}

public class SalaryHistoryItemDto
{
    public int StructureId { get; set; }
    public int Version { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public decimal MonthlyGrossSalary { get; set; }
    public decimal AnnualCTC { get; set; }
    public string? RevisionReason { get; set; }
    public decimal PercentageHike { get; set; }
    public bool IsActive { get; set; }
}
