namespace HRAttendance.Data.Models.Payroll;

public enum SalaryProrationBasis
{
    ActualCalendarDays = 1,
    ActualWorkingDays = 2,
    FixedDays = 3
}

public enum LOPCalculationBasis
{
    CalendarDays = 1,
    WorkingDays = 2,
    FixedDays = 3
}

public enum OvertimeBasis
{
    BasicSalary = 1,
    GrossSalary = 2,
    FixedHourlyRate = 3
}

public enum RoundingRule
{
    NearestWholeUnit = 1,
    RoundUp = 2,
    RoundDown = 3,
    TwoDecimals = 4
}

public enum StatutoryRuleType
{
    ProvidentFund = 1,
    ESI = 2,
    ProfessionalTax = 3,
    TDS = 4
}

public enum ComponentType
{
    Earning = 1,
    Deduction = 2,
    EmployerContribution = 3
}

public enum ComponentCalculationType
{
    FixedAmount = 1,
    PercentageOfBasic = 2,
    PercentageOfGross = 3,
    PercentageOfCTC = 4,
    ManualAmount = 5,
    Formula = 6
}

public enum PayrollRunType
{
    Regular = 1,
    OffCycle = 2,
    FinalSettlement = 3,
    Bonus = 4,
    Arrear = 5,
    Correction = 6
}

public enum PayrollPeriodStatus
{
    Draft = 1,
    Calculating = 2,
    Calculated = 3,
    UnderReview = 4,
    Approved = 5,
    Locked = 6,
    PaymentProcessing = 7,
    Paid = 8,
    Failed = 9,
    Cancelled = 10,
    Reopened = 11
}

public enum PayrollEmployeeStatus
{
    Pending = 1,
    Calculated = 2,
    Overridden = 3,
    Excluded = 4
}

public enum AdjustmentType
{
    Bonus = 1,
    Incentive = 2,
    Reimbursement = 3,
    Advance = 4,
    LoanRecovery = 5,
    NoticeRecovery = 6,
    ManualAdjustment = 7
}

public enum AdjustmentDirection
{
    Earning = 1,
    Deduction = 2
}

public enum AdjustmentStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Applied = 4,
    Cancelled = 5
}

public enum ArrearStatus
{
    Pending = 1,
    Calculated = 2,
    Applied = 3,
    Cancelled = 4
}

public enum ExceptionSeverity
{
    Critical = 1,
    Warning = 2,
    Information = 3
}

public enum PayslipStatus
{
    Draft = 1,
    Generated = 2,
    Published = 3,
    Cancelled = 4
}

public enum PayslipActionType
{
    View = 1,
    DownloadPdf = 2,
    AdminExport = 3
}

public enum PaymentStatus
{
    Pending = 1,
    Processing = 2,
    Paid = 3,
    Failed = 4,
    Reversed = 5,
    Cancelled = 6
}

public enum PaymentMethod
{
    BankTransfer = 1,
    Cheque = 2,
    Cash = 3,
    UPI = 4
}
