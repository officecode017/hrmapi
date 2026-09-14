using HRAttendance.Data.Models.Document;

namespace HRAttendance.Data.DTOs.Document;

public class EmployeeFolderDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int EmployeeId { get; set; }
    public int? ParentFolderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool IsSystemFolder { get; set; }
    public FolderType FolderType { get; set; }
    public int DocumentCount { get; set; }
    public int SubFolderCount { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class EmployeeDocumentDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int EmployeeId { get; set; }
    public int FolderId { get; set; }
    public string FolderName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DocumentSourceType SourceType { get; set; }
    public int? RelatedEntityId { get; set; }
    public bool IsSystemLocked { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedByName { get; set; }
}

public class BreadcrumbItemDto
{
    public int? FolderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}

public class EmployeeStorageBrowserDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string DesignationName { get; set; } = string.Empty;

    public EmployeeFolderDto? CurrentFolder { get; set; }
    public List<BreadcrumbItemDto> Breadcrumbs { get; set; } = new();
    public List<EmployeeFolderDto> Folders { get; set; } = new();
    public List<EmployeeDocumentDto> Documents { get; set; } = new();

    public int TotalFileCount { get; set; }
    public long TotalStorageSizeBytes { get; set; }
    public bool CanManage { get; set; }
}

public class CreateFolderRequestDto
{
    public int EmployeeId { get; set; }
    public int? ParentFolderId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class RenameFolderRequestDto
{
    public string NewName { get; set; } = string.Empty;
}

public class UpdateDocumentTitleRequestDto
{
    public string Title { get; set; } = string.Empty;
}
