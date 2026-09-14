using HRAttendance.Data.DTOs.Document;

namespace HRAttendance.Business.Interfaces.Documents;

public interface IEmployeeDocumentService
{
    /// <summary>
    /// Returns the contents of a directory (subfolders and files), current breadcrumb path,
    /// employee metadata, and permission flags for the requested folder.
    /// If folderId is null, returns the root level for that employee.
    /// </summary>
    Task<EmployeeStorageBrowserDto> GetStorageBrowserAsync(
        int organizationId, 
        int employeeId, 
        int? folderId, 
        bool isManagerOrAdmin, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a directory for an employee. If parentFolderId is null, creates at root level.
    /// </summary>
    Task<EmployeeFolderDto> CreateFolderAsync(
        int organizationId, 
        int employeeId, 
        int? parentFolderId, 
        string folderName, 
        int createdByUserId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renames a custom employee folder. System folders cannot be renamed.
    /// </summary>
    Task<EmployeeFolderDto> RenameFolderAsync(
        int organizationId, 
        int folderId, 
        string newName, 
        int modifiedByUserId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an employee folder and all documents inside it (including blobs).
    /// System folders cannot be deleted.
    /// </summary>
    Task<bool> DeleteFolderAsync(
        int organizationId, 
        int folderId, 
        int deletedByUserId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a file for an employee into the specified folder.
    /// </summary>
    Task<EmployeeDocumentDto> UploadDocumentAsync(
        int organizationId, 
        int employeeId, 
        int folderId, 
        string title, 
        string fileName, 
        string contentType, 
        Stream fileStream, 
        int uploadedByUserId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renames or updates metadata of a document.
    /// </summary>
    Task<EmployeeDocumentDto> UpdateDocumentTitleAsync(
        int organizationId, 
        int documentId, 
        string newTitle, 
        int modifiedByUserId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an employee document from the database and Azure Blob Storage.
    /// </summary>
    Task<bool> DeleteDocumentAsync(
        int organizationId, 
        int documentId, 
        int deletedByUserId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams a document from Azure Blob Storage for download.
    /// </summary>
    Task<(Stream Stream, string ContentType, string FileName)?> DownloadDocumentAsync(
        int organizationId, 
        int documentId, 
        int requestingEmployeeId, 
        bool isManagerOrAdmin, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Automatically renders the salary slip PDF and saves it into the employee's Finance &amp; Payroll folder.
    /// Called when an admin publishes payslips.
    /// </summary>
    Task<bool> ArchivePublishedPayslipAsync(int payslipId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures system default folders exist for an employee (e.g. Finance &amp; Payroll, Personal &amp; KYC, Contracts).
    /// </summary>
    Task EnsureSystemFoldersExistAsync(int organizationId, int employeeId, CancellationToken cancellationToken = default);
}
