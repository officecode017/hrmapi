using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HRAttendance.Business.Interfaces.Documents;
using HRAttendance.Business.Interfaces.Payroll;
using HRAttendance.Data;
using HRAttendance.Data.DTOs.Document;
using HRAttendance.Data.Models.Document;

namespace HRAttendance.Business.Services.Documents;

public class EmployeeDocumentService : IEmployeeDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IAzureBlobStorageService _blobStorage;
    private readonly IPayslipPdfGenerator _pdfGenerator;
    private readonly ILogger<EmployeeDocumentService> _logger;

    public EmployeeDocumentService(
        ApplicationDbContext context,
        IAzureBlobStorageService blobStorage,
        IPayslipPdfGenerator pdfGenerator,
        ILogger<EmployeeDocumentService> logger)
    {
        _context = context;
        _blobStorage = blobStorage;
        _pdfGenerator = pdfGenerator;
        _logger = logger;
    }

    public async Task EnsureSystemFoldersExistAsync(int organizationId, int employeeId, CancellationToken cancellationToken = default)
    {
        var existingFolders = await _context.EmployeeFolders
            .Where(f => f.OrganizationId == organizationId && f.EmployeeId == employeeId)
            .ToListAsync(cancellationToken);

        // 1. Finance & Payroll (Root)
        var financeFolder = existingFolders.FirstOrDefault(f => f.ParentFolderId == null && f.FolderType == FolderType.Finance);
        if (financeFolder == null)
        {
            financeFolder = new EmployeeFolder
            {
                OrganizationId = organizationId,
                EmployeeId = employeeId,
                ParentFolderId = null,
                Name = "Finance & Payroll",
                Path = "/Finance & Payroll",
                IsSystemFolder = true,
                FolderType = FolderType.Finance
            };
            _context.EmployeeFolders.Add(financeFolder);
            await _context.SaveChangesAsync(cancellationToken);
            existingFolders.Add(financeFolder);
        }

        // 2. Salary Slips (Subfolder under Finance & Payroll)
        var salarySlipsFolder = existingFolders.FirstOrDefault(f => f.ParentFolderId == financeFolder.Id && f.Name == "Salary Slips");
        if (salarySlipsFolder == null)
        {
            salarySlipsFolder = new EmployeeFolder
            {
                OrganizationId = organizationId,
                EmployeeId = employeeId,
                ParentFolderId = financeFolder.Id,
                Name = "Salary Slips",
                Path = "/Finance & Payroll/Salary Slips",
                IsSystemFolder = true,
                FolderType = FolderType.Finance
            };
            _context.EmployeeFolders.Add(salarySlipsFolder);
            await _context.SaveChangesAsync(cancellationToken);
            existingFolders.Add(salarySlipsFolder);
        }

        // 3. Personal & KYC (Root)
        var personalFolder = existingFolders.FirstOrDefault(f => f.ParentFolderId == null && f.FolderType == FolderType.Personal);
        if (personalFolder == null)
        {
            personalFolder = new EmployeeFolder
            {
                OrganizationId = organizationId,
                EmployeeId = employeeId,
                ParentFolderId = null,
                Name = "Personal & KYC",
                Path = "/Personal & KYC",
                IsSystemFolder = true,
                FolderType = FolderType.Personal
            };
            _context.EmployeeFolders.Add(personalFolder);
            await _context.SaveChangesAsync(cancellationToken);
            existingFolders.Add(personalFolder);
        }

        // 4. Contracts & Agreements (Root)
        var contractsFolder = existingFolders.FirstOrDefault(f => f.ParentFolderId == null && f.FolderType == FolderType.Contracts);
        if (contractsFolder == null)
        {
            contractsFolder = new EmployeeFolder
            {
                OrganizationId = organizationId,
                EmployeeId = employeeId,
                ParentFolderId = null,
                Name = "Contracts & Agreements",
                Path = "/Contracts & Agreements",
                IsSystemFolder = true,
                FolderType = FolderType.Contracts
            };
            _context.EmployeeFolders.Add(contractsFolder);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<EmployeeStorageBrowserDto> GetStorageBrowserAsync(
        int organizationId, 
        int employeeId, 
        int? folderId, 
        bool isManagerOrAdmin, 
        CancellationToken cancellationToken = default)
    {
        // Ensure default system folders exist
        await EnsureSystemFoldersExistAsync(organizationId, employeeId, cancellationToken);

        var employee = await _context.Employees
            .Include(e => e.ProfessionalDetails)
                .ThenInclude(pd => pd!.Department)
            .Include(e => e.ProfessionalDetails)
                .ThenInclude(pd => pd!.Designation)
            .FirstOrDefaultAsync(e => e.Id == employeeId && e.OrganizationId == organizationId, cancellationToken);

        if (employee == null)
        {
            throw new KeyNotFoundException($"Employee with ID {employeeId} not found in this organization.");
        }

        var allEmployeeFolders = await _context.EmployeeFolders
            .Where(f => f.OrganizationId == organizationId && f.EmployeeId == employeeId)
            .ToListAsync(cancellationToken);

        var allDocuments = await _context.EmployeeDocuments
            .Where(d => d.OrganizationId == organizationId && d.EmployeeId == employeeId)
            .ToListAsync(cancellationToken);

        EmployeeFolder? currentFolder = null;
        var breadcrumbs = new List<BreadcrumbItemDto>
        {
            new BreadcrumbItemDto { FolderId = null, Name = "All Documents", Path = "/" }
        };

        if (folderId.HasValue && folderId.Value > 0)
        {
            currentFolder = allEmployeeFolders.FirstOrDefault(f => f.Id == folderId.Value);
            if (currentFolder == null)
            {
                throw new KeyNotFoundException("The requested folder does not exist or has been deleted.");
            }

            // Build breadcrumb hierarchy
            var pathStack = new Stack<EmployeeFolder>();
            var cursor = currentFolder;
            while (cursor != null)
            {
                pathStack.Push(cursor);
                cursor = cursor.ParentFolderId.HasValue
                    ? allEmployeeFolders.FirstOrDefault(f => f.Id == cursor.ParentFolderId.Value)
                    : null;
            }

            while (pathStack.Count > 0)
            {
                var f = pathStack.Pop();
                breadcrumbs.Add(new BreadcrumbItemDto
                {
                    FolderId = f.Id,
                    Name = f.Name,
                    Path = f.Path
                });
            }
        }

        // Subfolders in current level
        var subFolders = allEmployeeFolders
            .Where(f => f.ParentFolderId == folderId)
            .OrderByDescending(f => f.IsSystemFolder)
            .ThenBy(f => f.Name)
            .Select(f => new EmployeeFolderDto
            {
                Id = f.Id,
                OrganizationId = f.OrganizationId,
                EmployeeId = f.EmployeeId,
                ParentFolderId = f.ParentFolderId,
                Name = f.Name,
                Path = f.Path,
                IsSystemFolder = f.IsSystemFolder,
                FolderType = f.FolderType,
                DocumentCount = allDocuments.Count(d => d.FolderId == f.Id),
                SubFolderCount = allEmployeeFolders.Count(sf => sf.ParentFolderId == f.Id),
                CreatedAt = f.CreatedAt
            }).ToList();

        // Files in current level
        var documents = (folderId.HasValue && folderId.Value > 0)
            ? allDocuments
                .Where(d => d.FolderId == folderId.Value)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new EmployeeDocumentDto
                {
                    Id = d.Id,
                    OrganizationId = d.OrganizationId,
                    EmployeeId = d.EmployeeId,
                    FolderId = d.FolderId,
                    FolderName = currentFolder?.Name ?? string.Empty,
                    Title = d.Title,
                    OriginalFileName = d.OriginalFileName,
                    FileExtension = d.FileExtension,
                    ContentType = d.ContentType,
                    FileSizeBytes = d.FileSizeBytes,
                    SourceType = d.SourceType,
                    RelatedEntityId = d.RelatedEntityId,
                    IsSystemLocked = d.IsSystemLocked,
                    CreatedAt = d.CreatedAt
                }).ToList()
            : new List<EmployeeDocumentDto>();

        return new EmployeeStorageBrowserDto
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.FullName,
            EmployeeCode = employee.EmployeeCode,
            DepartmentName = employee.ProfessionalDetails?.Department?.Name ?? string.Empty,
            DesignationName = employee.ProfessionalDetails?.Designation?.Name ?? string.Empty,
            CurrentFolder = currentFolder != null ? new EmployeeFolderDto
            {
                Id = currentFolder.Id,
                OrganizationId = currentFolder.OrganizationId,
                EmployeeId = currentFolder.EmployeeId,
                ParentFolderId = currentFolder.ParentFolderId,
                Name = currentFolder.Name,
                Path = currentFolder.Path,
                IsSystemFolder = currentFolder.IsSystemFolder,
                FolderType = currentFolder.FolderType,
                DocumentCount = allDocuments.Count(d => d.FolderId == currentFolder.Id),
                SubFolderCount = allEmployeeFolders.Count(sf => sf.ParentFolderId == currentFolder.Id),
                CreatedAt = currentFolder.CreatedAt
            } : null,
            Breadcrumbs = breadcrumbs,
            Folders = subFolders,
            Documents = documents,
            TotalFileCount = allDocuments.Count,
            TotalStorageSizeBytes = allDocuments.Sum(d => d.FileSizeBytes),
            CanManage = isManagerOrAdmin
        };
    }

    public async Task<EmployeeFolderDto> CreateFolderAsync(
        int organizationId, 
        int employeeId, 
        int? parentFolderId, 
        string folderName, 
        int createdByUserId, 
        CancellationToken cancellationToken = default)
    {
        var cleanName = folderName.Trim();
        if (string.IsNullOrWhiteSpace(cleanName))
        {
            throw new ArgumentException("Folder name cannot be empty.", nameof(folderName));
        }

        string folderPath;
        if (parentFolderId.HasValue && parentFolderId.Value > 0)
        {
            var parent = await _context.EmployeeFolders
                .FirstOrDefaultAsync(f => f.Id == parentFolderId.Value && f.OrganizationId == organizationId && f.EmployeeId == employeeId, cancellationToken);
            if (parent == null)
            {
                throw new KeyNotFoundException("Parent folder does not exist.");
            }
            folderPath = $"{parent.Path}/{cleanName}";
        }
        else
        {
            parentFolderId = null;
            folderPath = $"/{cleanName}";
        }

        var exists = await _context.EmployeeFolders
            .AnyAsync(f => f.OrganizationId == organizationId && f.EmployeeId == employeeId && f.ParentFolderId == parentFolderId && f.Name == cleanName, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"A folder named '{cleanName}' already exists at this location.");
        }

        var newFolder = new EmployeeFolder
        {
            OrganizationId = organizationId,
            EmployeeId = employeeId,
            ParentFolderId = parentFolderId,
            Name = cleanName,
            Path = folderPath,
            IsSystemFolder = false,
            FolderType = FolderType.General,
            CreatedBy = createdByUserId
        };

        _context.EmployeeFolders.Add(newFolder);
        await _context.SaveChangesAsync(cancellationToken);

        return new EmployeeFolderDto
        {
            Id = newFolder.Id,
            OrganizationId = newFolder.OrganizationId,
            EmployeeId = newFolder.EmployeeId,
            ParentFolderId = newFolder.ParentFolderId,
            Name = newFolder.Name,
            Path = newFolder.Path,
            IsSystemFolder = newFolder.IsSystemFolder,
            FolderType = newFolder.FolderType,
            DocumentCount = 0,
            SubFolderCount = 0,
            CreatedAt = newFolder.CreatedAt
        };
    }

    public async Task<EmployeeFolderDto> RenameFolderAsync(
        int organizationId, 
        int folderId, 
        string newName, 
        int modifiedByUserId, 
        CancellationToken cancellationToken = default)
    {
        var folder = await _context.EmployeeFolders
            .FirstOrDefaultAsync(f => f.Id == folderId && f.OrganizationId == organizationId, cancellationToken);

        if (folder == null)
        {
            throw new KeyNotFoundException("Folder not found.");
        }

        if (folder.IsSystemFolder)
        {
            throw new InvalidOperationException("System folders cannot be renamed.");
        }

        var cleanName = newName.Trim();
        if (string.IsNullOrWhiteSpace(cleanName))
        {
            throw new ArgumentException("Folder name cannot be empty.", nameof(newName));
        }

        var duplicateExists = await _context.EmployeeFolders
            .AnyAsync(f => f.Id != folderId && f.OrganizationId == organizationId && f.EmployeeId == folder.EmployeeId && f.ParentFolderId == folder.ParentFolderId && f.Name == cleanName, cancellationToken);

        if (duplicateExists)
        {
            throw new InvalidOperationException($"A sibling folder named '{cleanName}' already exists.");
        }

        folder.Name = cleanName;
        if (folder.ParentFolderId.HasValue)
        {
            var parent = await _context.EmployeeFolders.FindAsync(new object[] { folder.ParentFolderId.Value }, cancellationToken);
            folder.Path = parent != null ? $"{parent.Path}/{cleanName}" : $"/{cleanName}";
        }
        else
        {
            folder.Path = $"/{cleanName}";
        }
        folder.ModifiedBy = modifiedByUserId;

        await _context.SaveChangesAsync(cancellationToken);

        return new EmployeeFolderDto
        {
            Id = folder.Id,
            OrganizationId = folder.OrganizationId,
            EmployeeId = folder.EmployeeId,
            ParentFolderId = folder.ParentFolderId,
            Name = folder.Name,
            Path = folder.Path,
            IsSystemFolder = folder.IsSystemFolder,
            FolderType = folder.FolderType,
            CreatedAt = folder.CreatedAt
        };
    }

    public async Task<bool> DeleteFolderAsync(
        int organizationId, 
        int folderId, 
        int deletedByUserId, 
        CancellationToken cancellationToken = default)
    {
        var folder = await _context.EmployeeFolders
            .FirstOrDefaultAsync(f => f.Id == folderId && f.OrganizationId == organizationId, cancellationToken);

        if (folder == null)
        {
            throw new KeyNotFoundException("Folder not found.");
        }

        if (folder.IsSystemFolder)
        {
            throw new InvalidOperationException("System default folders cannot be deleted.");
        }

        // Fetch all descendant folders recursively
        var allFolders = await _context.EmployeeFolders
            .Where(f => f.OrganizationId == organizationId && f.EmployeeId == folder.EmployeeId)
            .ToListAsync(cancellationToken);

        var targetFolderIds = new HashSet<int> { folderId };
        bool added;
        do
        {
            added = false;
            foreach (var f in allFolders)
            {
                if (f.ParentFolderId.HasValue && targetFolderIds.Contains(f.ParentFolderId.Value) && targetFolderIds.Add(f.Id))
                {
                    added = true;
                }
            }
        } while (added);

        // Fetch documents in all targeted folders
        var documents = await _context.EmployeeDocuments
            .Where(d => targetFolderIds.Contains(d.FolderId) && d.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        // Delete blobs from Azure Blob Storage
        foreach (var doc in documents)
        {
            try
            {
                await _blobStorage.DeleteBlobAsync(doc.BlobPath, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete blob {BlobPath} while removing folder {FolderId}", doc.BlobPath, folderId);
            }
            _context.EmployeeDocuments.Remove(doc);
        }

        // Delete folders
        var foldersToDelete = allFolders.Where(f => targetFolderIds.Contains(f.Id)).ToList();
        _context.EmployeeFolders.RemoveRange(foldersToDelete);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<EmployeeDocumentDto> UploadDocumentAsync(
        int organizationId, 
        int employeeId, 
        int folderId, 
        string title, 
        string fileName, 
        string contentType, 
        Stream fileStream, 
        int uploadedByUserId, 
        CancellationToken cancellationToken = default)
    {
        var folder = await _context.EmployeeFolders
            .FirstOrDefaultAsync(f => f.Id == folderId && f.OrganizationId == organizationId && f.EmployeeId == employeeId, cancellationToken);

        if (folder == null)
        {
            throw new KeyNotFoundException("Target folder does not exist for this employee.");
        }

        var safeFileName = Path.GetFileName(fileName);
        var extension = Path.GetExtension(safeFileName);
        var documentTitle = string.IsNullOrWhiteSpace(title) 
            ? Path.GetFileNameWithoutExtension(safeFileName) 
            : title.Trim();

        var blobPath = $"org-{organizationId}/emp-{employeeId}/fld-{folderId}/{Guid.NewGuid():N}_{safeFileName}";
        var sizeBytes = fileStream.Length;

        // Upload to Azure Blob Storage
        await _blobStorage.UploadBlobAsync(blobPath, fileStream, contentType, cancellationToken);

        var document = new EmployeeDocument
        {
            OrganizationId = organizationId,
            EmployeeId = employeeId,
            FolderId = folderId,
            Title = documentTitle,
            OriginalFileName = safeFileName,
            FileExtension = extension,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            FileSizeBytes = sizeBytes,
            BlobPath = blobPath,
            SourceType = DocumentSourceType.ManualUpload,
            IsSystemLocked = false,
            CreatedBy = uploadedByUserId
        };

        _context.EmployeeDocuments.Add(document);
        await _context.SaveChangesAsync(cancellationToken);

        return new EmployeeDocumentDto
        {
            Id = document.Id,
            OrganizationId = document.OrganizationId,
            EmployeeId = document.EmployeeId,
            FolderId = document.FolderId,
            FolderName = folder.Name,
            Title = document.Title,
            OriginalFileName = document.OriginalFileName,
            FileExtension = document.FileExtension,
            ContentType = document.ContentType,
            FileSizeBytes = document.FileSizeBytes,
            SourceType = document.SourceType,
            RelatedEntityId = document.RelatedEntityId,
            IsSystemLocked = document.IsSystemLocked,
            CreatedAt = document.CreatedAt
        };
    }

    public async Task<EmployeeDocumentDto> UpdateDocumentTitleAsync(
        int organizationId, 
        int documentId, 
        string newTitle, 
        int modifiedByUserId, 
        CancellationToken cancellationToken = default)
    {
        var document = await _context.EmployeeDocuments
            .Include(d => d.Folder)
            .FirstOrDefaultAsync(d => d.Id == documentId && d.OrganizationId == organizationId, cancellationToken);

        if (document == null)
        {
            throw new KeyNotFoundException("Document not found.");
        }

        document.Title = newTitle.Trim();
        document.ModifiedBy = modifiedByUserId;
        await _context.SaveChangesAsync(cancellationToken);

        return new EmployeeDocumentDto
        {
            Id = document.Id,
            OrganizationId = document.OrganizationId,
            EmployeeId = document.EmployeeId,
            FolderId = document.FolderId,
            FolderName = document.Folder?.Name ?? string.Empty,
            Title = document.Title,
            OriginalFileName = document.OriginalFileName,
            FileExtension = document.FileExtension,
            ContentType = document.ContentType,
            FileSizeBytes = document.FileSizeBytes,
            SourceType = document.SourceType,
            RelatedEntityId = document.RelatedEntityId,
            IsSystemLocked = document.IsSystemLocked,
            CreatedAt = document.CreatedAt
        };
    }

    public async Task<bool> DeleteDocumentAsync(
        int organizationId, 
        int documentId, 
        int deletedByUserId, 
        CancellationToken cancellationToken = default)
    {
        var document = await _context.EmployeeDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId && d.OrganizationId == organizationId, cancellationToken);

        if (document == null)
        {
            throw new KeyNotFoundException("Document not found.");
        }

        if (document.IsSystemLocked)
        {
            throw new InvalidOperationException("System-generated documents (e.g. published salary slips) cannot be deleted directly.");
        }

        try
        {
            await _blobStorage.DeleteBlobAsync(document.BlobPath, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete blob at {BlobPath} for document {DocumentId}", document.BlobPath, documentId);
        }

        _context.EmployeeDocuments.Remove(document);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<(Stream Stream, string ContentType, string FileName)?> DownloadDocumentAsync(
        int organizationId, 
        int documentId, 
        int requestingEmployeeId, 
        bool isManagerOrAdmin, 
        CancellationToken cancellationToken = default)
    {
        var document = await _context.EmployeeDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId && d.OrganizationId == organizationId, cancellationToken);

        if (document == null)
        {
            return null;
        }

        // Authorization check: Employee can only download their own documents
        if (!isManagerOrAdmin && document.EmployeeId != requestingEmployeeId)
        {
            throw new UnauthorizedAccessException("You are not authorized to download this document.");
        }

        var stream = await _blobStorage.DownloadBlobAsync(document.BlobPath, cancellationToken);
        if (stream == null)
        {
            return null;
        }

        return (stream, document.ContentType, document.OriginalFileName);
    }

    public async Task<bool> ArchivePublishedPayslipAsync(int payslipId, CancellationToken cancellationToken = default)
    {
        var payslip = await _context.Payslips
            .Include(p => p.PayrollEmployee)
                .ThenInclude(pe => pe.Period)
            .FirstOrDefaultAsync(p => p.Id == payslipId, cancellationToken);

        if (payslip == null)
        {
            _logger.LogWarning("Cannot archive payslip {PayslipId}: Payslip not found.", payslipId);
            return false;
        }

        var organizationId = payslip.PayrollEmployee.OrganizationId;
        var employeeId = payslip.EmployeeId;

        // Ensure system folders exist
        await EnsureSystemFoldersExistAsync(organizationId, employeeId, cancellationToken);

        // Locate Salary Slips folder
        var salarySlipsFolder = await _context.EmployeeFolders
            .FirstOrDefaultAsync(f => f.OrganizationId == organizationId && f.EmployeeId == employeeId && f.Name == "Salary Slips", cancellationToken);

        if (salarySlipsFolder == null)
        {
            salarySlipsFolder = await _context.EmployeeFolders
                .FirstOrDefaultAsync(f => f.OrganizationId == organizationId && f.EmployeeId == employeeId && f.FolderType == FolderType.Finance, cancellationToken);
        }

        if (salarySlipsFolder == null)
        {
            _logger.LogError("Failed to locate or create Salary Slips folder for employee {EmployeeId}", employeeId);
            return false;
        }

        // Generate the PDF
        byte[] pdfBytes;
        try
        {
            pdfBytes = await _pdfGenerator.GeneratePayslipPdfAsync(payslipId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render PDF for payslip {PayslipId} during archiving", payslipId);
            return false;
        }

        var periodDesc = payslip.PayrollEmployee.Period?.Description ?? $"{payslip.PayrollEmployee.Period?.Month}_{payslip.PayrollEmployee.Period?.Year}";
        var fileName = $"Payslip_{periodDesc.Replace(" ", "_")}_{payslip.PayslipNumber}.pdf";
        var title = $"Payslip - {periodDesc}";
        var blobPath = $"org-{organizationId}/emp-{employeeId}/fld-{salarySlipsFolder.Id}/payslip_{payslip.Id}_{payslip.PayslipNumber}.pdf";

        // Upload PDF bytes to Azure Blob Storage
        using (var ms = new MemoryStream(pdfBytes))
        {
            await _blobStorage.UploadBlobAsync(blobPath, ms, "application/pdf", cancellationToken);
        }

        // Check if document record already exists for this payslip
        var existingDoc = await _context.EmployeeDocuments
            .FirstOrDefaultAsync(d => d.OrganizationId == organizationId && d.EmployeeId == employeeId && d.SourceType == DocumentSourceType.Payslip && d.RelatedEntityId == payslipId, cancellationToken);

        if (existingDoc != null)
        {
            existingDoc.BlobPath = blobPath;
            existingDoc.FileSizeBytes = pdfBytes.Length;
            existingDoc.Title = title;
            existingDoc.OriginalFileName = fileName;
            existingDoc.ModifiedAt = DateTime.UtcNow;
        }
        else
        {
            var newDoc = new EmployeeDocument
            {
                OrganizationId = organizationId,
                EmployeeId = employeeId,
                FolderId = salarySlipsFolder.Id,
                Title = title,
                OriginalFileName = fileName,
                FileExtension = ".pdf",
                ContentType = "application/pdf",
                FileSizeBytes = pdfBytes.Length,
                BlobPath = blobPath,
                SourceType = DocumentSourceType.Payslip,
                RelatedEntityId = payslipId,
                IsSystemLocked = true
            };
            _context.EmployeeDocuments.Add(newDoc);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Successfully archived published payslip {PayslipId} into employee storage folder {FolderId}", payslipId, salarySlipsFolder.Id);
        return true;
    }
}
