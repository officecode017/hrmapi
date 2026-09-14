namespace HRAttendance.Data.Models.Document;

public enum DocumentSourceType
{
    ManualUpload = 0,
    Payslip = 1,
    TaxForm = 2,
    Contract = 3,
    Kyc = 4
}

public enum FolderType
{
    General = 0,
    Finance = 1,
    Personal = 2,
    Contracts = 3
}
