using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using HRAttendance.Business.Interfaces;

namespace HRAttendance.Business.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalFileStorageService> _logger;

    private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5MB
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".svg", ".gif"
    };

    public LocalFileStorageService(IWebHostEnvironment environment, ILogger<LocalFileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string subfolder, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("Uploaded file is empty.");
        }

        if (file.Length > MaxFileSizeInBytes)
        {
            throw new InvalidOperationException($"File size exceeds the maximum limit of {MaxFileSizeInBytes / (1024 * 1024)}MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"Invalid file extension '{extension}'. Allowed extensions: {string.Join(", ", AllowedExtensions)}");
        }

        // Sanitize subfolder name to prevent directory traversal
        var sanitizedSubfolder = Path.GetFileName(subfolder);
        if (string.IsNullOrWhiteSpace(sanitizedSubfolder))
        {
            sanitizedSubfolder = "misc";
        }

        var uploadsRoot = Path.Combine(_environment.ContentRootPath, "uploads");
        var targetFolder = Path.Combine(uploadsRoot, sanitizedSubfolder);

        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
        var destinationPath = Path.Combine(targetFolder, uniqueFileName);

        await using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(fileStream, cancellationToken);
        }

        _logger.LogInformation("Saved uploaded file '{FileName}' to '{DestinationPath}'", file.FileName, destinationPath);

        // Return relative web URL
        return $"/uploads/{sanitizedSubfolder}/{uniqueFileName}";
    }

    public bool DeleteFile(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return false;

        try
        {
            // Normalize path
            var cleanPath = relativePath.TrimStart('/', '\\');
            if (!cleanPath.StartsWith("uploads", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var uploadsRoot = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "uploads"));
            var targetFilePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, cleanPath));

            // Prevent path traversal outside of uploads directory
            if (!targetFilePath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Path traversal attempt blocked: {Path}", relativePath);
                return false;
            }

            if (File.Exists(targetFilePath))
            {
                File.Delete(targetFilePath);
                _logger.LogInformation("Deleted file at: {FilePath}", targetFilePath);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete file at path: {Path}", relativePath);
        }

        return false;
    }
}
