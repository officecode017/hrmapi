using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Data.DTOs.Payroll;

public class PayrollEmployeeListDto
{
    public int Id { get; set; }
    public int PayrollPeriodId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string DesignationName { get; set; } = string.Empty;
    public decimal BaseMonthlyGross { get; set; }
    public decimal ProratedGross { get; set; }
    public decimal LOPDays { get; set; }
    public decimal LOPDeduction { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal GrossEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetPay { get; set; }
    public PayrollEmployeeStatus Status { get; set; }
}

public class PayrollEmployeeDetailDto
{
    public int Id { get; set; }
    public int PayrollPeriodId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string DesignationName { get; set; } = string.Empty;
    public string PanNumber { get; set; } = string.Empty;
    public string UanNumber { get; set; } = string.Empty;
    public string BankAccountNumber { get; set; } = string.Empty;
    public string BankIfscCode { get; set; } = string.Empty;
    public int CalculationVersion { get; set; }

    public int EligibleEmploymentDays { get; set; }
    public int CalendarDaysInMonth { get; set; }
    public decimal WorkingDays { get; set; }
    public decimal PresentDays { get; set; }
    public decimal PaidLeaveDays { get; set; }
    public decimal HalfDays { get; set; }
    public decimal LOPDays { get; set; }
    public decimal ApprovedOvertimeHours { get; set; }

    public decimal BaseMonthlyGross { get; set; }
    public decimal ProratedGross { get; set; }
    public decimal LOPDeduction { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal AdjustmentsTotal { get; set; }
    public decimal ArrearsTotal { get; set; }
    public decimal GrossEarnings { get; set; }
    public decimal StatutoryDeductions { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal EmployerContributions { get; set; }
    public decimal NetPay { get; set; }

    public PayrollEmployeeStatus Status { get; set; }
    public DateTimeOffset CalculatedAt { get; set; }

    public List<PayrollSalarySliceDto> Slices { get; set; } = new();
    public List<PayrollItemDto> Earnings { get; set; } = new();
    public List<PayrollItemDto> Deductions { get; set; } = new();
    public List<PayrollItemDto> EmployerContributionsList { get; set; } = new();
}

public class PayrollSalarySliceDto
{
    public int Id { get; set; }
    public int SalaryStructureId { get; set; }
    public int SalaryStructureVersion { get; set; }
    public DateOnly SliceStartDate { get; set; }
    public DateOnly SliceEndDate { get; set; }
    public int TotalCalendarDaysInSlice { get; set; }
    public int EligibleDaysInSlice { get; set; }
    public decimal MonthlyGrossInSlice { get; set; }
    public decimal ProratedGrossInSlice { get; set; }
    public string SliceNotes { get; set; } = string.Empty;
}

public class PayrollItemDto
{
    public int Id { get; set; }
    public string ComponentCode { get; set; } = string.Empty;
    public string ComponentName { get; set; } = string.Empty;
    public ComponentType ComponentType { get; set; }
    public int CalculationOrder { get; set; }
    public ComponentCalculationType CalculationType { get; set; }
    public decimal CalculationBase { get; set; }
    public decimal CalculationRate { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal ProratedAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string CalculationFormula { get; set; } = string.Empty;
    public string CalculationNotes { get; set; } = string.Empty;
}

public class PayrollAttendanceBreakdownDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int TotalCalendarDays { get; set; }
    public int EligibleDays { get; set; }
    public decimal WorkingDays { get; set; }
    public decimal PresentDays { get; set; }
    public decimal PaidLeaveDays { get; set; }
    public decimal HalfDays { get; set; }
    public decimal LOPDays { get; set; }
    public decimal ApprovedOvertimeHours { get; set; }
    public List<AttendanceDailyDetailDto> DailyEntries { get; set; } = new();
}

public class AttendanceDailyDetailDto
{
    public DateOnly Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}

public class PayrollDerivationLogDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public List<DerivationStepDto> Steps { get; set; } = new();
}

public class DerivationStepDto
{
    public int StepNumber { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Formula { get; set; } = string.Empty;
    public decimal ResultValue { get; set; }
}
