using HRAttendance.Data.Models.Common;

namespace HRAttendance.Data.Models.Payroll;

public class PayrollSalarySlice : AuditableEntity
{
    public int PayrollEmployeeId { get; set; }
    public int SalaryStructureId { get; set; }
    public int SalaryStructureVersion { get; set; }
    public DateOnly SliceStartDate { get; set; }
    public DateOnly SliceEndDate { get; set; }
    public int TotalCalendarDaysInSlice { get; set; }
    public int EligibleDaysInSlice { get; set; }
    public decimal MonthlyGrossInSlice { get; set; }
    public decimal ProratedGrossInSlice { get; set; }
    public string SliceNotes { get; set; } = string.Empty;

    public virtual PayrollEmployee PayrollEmployee { get; set; } = null!;
    public virtual EmployeeSalaryStructure SalaryStructure { get; set; } = null!;
}
