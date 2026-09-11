using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Data.DTOs.Payroll;

public class CreateSalaryComponentDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ComponentType Type { get; set; }
    public ComponentCalculationType CalculationType { get; set; }
    public int CalculationOrder { get; set; }
    public bool IsTaxable { get; set; } = true;
    public bool IsStatutory { get; set; } = false;
}

public class UpdateSalaryComponentDto
{
    public string Name { get; set; } = string.Empty;
    public ComponentType Type { get; set; }
    public ComponentCalculationType CalculationType { get; set; }
    public int CalculationOrder { get; set; }
    public bool IsTaxable { get; set; }
    public bool IsStatutory { get; set; }
    public bool IsActive { get; set; }
}

public class SalaryComponentDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ComponentType Type { get; set; }
    public ComponentCalculationType CalculationType { get; set; }
    public int CalculationOrder { get; set; }
    public bool IsTaxable { get; set; }
    public bool IsStatutory { get; set; }
    public bool IsActive { get; set; }
}
