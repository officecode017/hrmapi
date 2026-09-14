using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRAttendance.API.Extensions;
using HRAttendance.Business.BusinessRules;
using HRAttendance.Business.Interfaces.Documents;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Document;

namespace HRAttendance.API.Controllers.Document;

[ApiController]
[Authorize]
public class EmployeeDocumentsController : ControllerBase
{
    private readonly IEmployeeDocumentService _documentService;
    private readonly ILogger<EmployeeDocumentsController> _logger;

    public EmployeeDocumentsController(
        IEmployeeDocumentService documentService,
        ILogger<EmployeeDocumentsController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    /// <summary>
    /// Employee Self-Service: Browse own files and auto-created folders.
    /// Employees have strictly read-only access.
    /// </summary>
    [HttpGet("api/documents/my-browser")]
    [ProducesResponseType(typeof(ApiResponseDto<EmployeeStorageBrowserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyStorageBrowser([FromQuery] int? folderId, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var employeeId = User.GetEmployeeId();

        try
        {
            var browser = await _documentService.GetStorageBrowserAsync(
                orgId, 
                employeeId, 
                folderId, 
                isManagerOrAdmin: false, 
                cancellationToken);

            return Ok(ApiResponseDto<EmployeeStorageBrowserDto>.Ok(browser));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponseDto<EmployeeStorageBrowserDto>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Manager / Admin: Browse employee document directory and files.
    /// </summary>
    [HttpGet("api/employees/{employeeId:int}/documents/browser")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin},{ApplicationRoles.Manager}")]
    [ProducesResponseType(typeof(ApiResponseDto<EmployeeStorageBrowserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployeeStorageBrowser(
        int employeeId, 
        [FromQuery] int? folderId, 
        CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var isManagerOrAdmin = User.IsManagerOrAdmin();

        try
        {
            var browser = await _documentService.GetStorageBrowserAsync(
                orgId, 
                employeeId, 
                folderId, 
                isManagerOrAdmin, 
                cancellationToken);

            return Ok(ApiResponseDto<EmployeeStorageBrowserDto>.Ok(browser));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponseDto<EmployeeStorageBrowserDto>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Manager / Admin: Create a directory for an employee.
    /// </summary>
    [HttpPost("api/employees/{employeeId:int}/documents/folders")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin},{ApplicationRoles.Manager}")]
    [ProducesResponseType(typeof(ApiResponseDto<EmployeeFolderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFolder(
        int employeeId, 
        [FromBody] CreateFolderRequestDto request, 
        CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var currentUserId = User.GetEmployeeId();

        try
        {
            var folder = await _documentService.CreateFolderAsync(
                orgId, 
                employeeId, 
                request.ParentFolderId, 
                request.Name, 
                currentUserId, 
                cancellationToken);

            return Ok(ApiResponseDto<EmployeeFolderDto>.Ok(folder, "Folder created successfully."));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or KeyNotFoundException)
        {
            return BadRequest(ApiResponseDto<EmployeeFolderDto>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Manager / Admin: Rename an existing directory.
    /// </summary>
    [HttpPut("api/documents/folders/{folderId:int}/rename")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin},{ApplicationRoles.Manager}")]
    [ProducesResponseType(typeof(ApiResponseDto<EmployeeFolderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RenameFolder(
        int folderId, 
        [FromBody] RenameFolderRequestDto request, 
        CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var currentUserId = User.GetEmployeeId();

        try
        {
            var folder = await _documentService.RenameFolderAsync(
                orgId, 
                folderId, 
                request.NewName, 
                currentUserId, 
                cancellationToken);

            return Ok(ApiResponseDto<EmployeeFolderDto>.Ok(folder, "Folder renamed successfully."));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or KeyNotFoundException)
        {
            return BadRequest(ApiResponseDto<EmployeeFolderDto>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Manager / Admin: Delete a directory and all documents contained inside it.
    /// </summary>
    [HttpDelete("api/documents/folders/{folderId:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin},{ApplicationRoles.Manager}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteFolder(int folderId, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var currentUserId = User.GetEmployeeId();

        try
        {
            var success = await _documentService.DeleteFolderAsync(
                orgId, 
                folderId, 
                currentUserId, 
                cancellationToken);

            return Ok(ApiResponseDto<bool>.Ok(success, "Folder and its contents deleted successfully."));
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            return BadRequest(ApiResponseDto<bool>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Manager / Admin: Upload a file for an employee into a specified directory.
    /// </summary>
    [HttpPost("api/employees/{employeeId:int}/documents/upload")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin},{ApplicationRoles.Manager}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponseDto<EmployeeDocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadDocument(
        int employeeId, 
        [FromForm] int folderId, 
        [FromForm] string? title, 
        IFormFile file, 
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponseDto<EmployeeDocumentDto>.Fail("A valid file must be provided."));
        }

        var orgId = User.GetOrganizationIdOrDefault();
        var currentUserId = User.GetEmployeeId();

        try
        {
            using var stream = file.OpenReadStream();
            var doc = await _documentService.UploadDocumentAsync(
                orgId, 
                employeeId, 
                folderId, 
                title ?? string.Empty, 
                file.FileName, 
                file.ContentType, 
                stream, 
                currentUserId, 
                cancellationToken);

            return Ok(ApiResponseDto<EmployeeDocumentDto>.Ok(doc, "Document uploaded successfully."));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or KeyNotFoundException)
        {
            return BadRequest(ApiResponseDto<EmployeeDocumentDto>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Manager / Admin: Update document title.
    /// </summary>
    [HttpPut("api/documents/{documentId:int}/title")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin},{ApplicationRoles.Manager}")]
    [ProducesResponseType(typeof(ApiResponseDto<EmployeeDocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateDocumentTitle(
        int documentId, 
        [FromBody] UpdateDocumentTitleRequestDto request, 
        CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var currentUserId = User.GetEmployeeId();

        try
        {
            var doc = await _documentService.UpdateDocumentTitleAsync(
                orgId, 
                documentId, 
                request.Title, 
                currentUserId, 
                cancellationToken);

            return Ok(ApiResponseDto<EmployeeDocumentDto>.Ok(doc, "Document title updated successfully."));
        }
        catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException)
        {
            return BadRequest(ApiResponseDto<EmployeeDocumentDto>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Manager / Admin: Delete a document from employee storage and Azure Blob Storage.
    /// </summary>
    [HttpDelete("api/documents/{documentId:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin},{ApplicationRoles.Manager}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteDocument(int documentId, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var currentUserId = User.GetEmployeeId();

        try
        {
            var success = await _documentService.DeleteDocumentAsync(
                orgId, 
                documentId, 
                currentUserId, 
                cancellationToken);

            return Ok(ApiResponseDto<bool>.Ok(success, "Document deleted successfully."));
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            return BadRequest(ApiResponseDto<bool>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Download / Stream document. Available to both Employee (for their own files) and Admin/Manager.
    /// </summary>
    [HttpGet("api/documents/{documentId:int}/download")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DownloadDocument(int documentId, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var currentUserId = User.GetEmployeeId();
        var isManagerOrAdmin = User.IsManagerOrAdmin();

        try
        {
            var download = await _documentService.DownloadDocumentAsync(
                orgId, 
                documentId, 
                currentUserId, 
                isManagerOrAdmin, 
                cancellationToken);

            if (download == null)
            {
                return NotFound(ApiResponseDto<string>.Fail("Document or storage blob could not be found."));
            }

            return File(download.Value.Stream, download.Value.ContentType, download.Value.FileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponseDto<string>.Fail(ex.Message));
        }
    }
}
