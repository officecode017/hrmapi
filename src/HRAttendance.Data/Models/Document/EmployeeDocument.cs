using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Employee;

namespace HRAttendance.Data.Models.Document;

public class EmployeeDocument : AuditableEntity
{
    public int OrganizationId { get; set; }
    public int EmployeeId { get; set; }
    public int FolderId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Relative path/blob name inside the Azure Blob Storage container.
    /// Format: org-{orgId}/emp-{empId}/{folderId}/{guid}_{fileName}
    /// </summary>
    public string BlobPath { get; set; } = string.Empty;

    public DocumentSourceType SourceType { get; set; } = DocumentSourceType.ManualUpload;

    /// <summary>
    /// Optional foreign key to related entity (e.g. PayslipId if SourceType == Payslip).
    /// </summary>
    public int? RelatedEntityId { get; set; }

    /// <summary>
    /// If true, this is a system-generated document (such as published salary slip)
    /// which cannot be deleted by standard browser operations.
    /// </summary>
    public bool IsSystemLocked { get; set; }

    // Navigation
    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual Models.Employee.Employee Employee { get; set; } = null!;
    public virtual EmployeeFolder Folder { get; set; } = null!;
}
