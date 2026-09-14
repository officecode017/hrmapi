using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using HRAttendance.Business.Interfaces.Documents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HRAttendance.Business.Services.Documents;

public class AzureBlobStorageService : IAzureBlobStorageService
{
    private readonly BlobServiceClient? _blobServiceClient;
    private readonly BlobContainerClient? _containerClient;
    private readonly ILogger<AzureBlobStorageService> _logger;
    private readonly string _containerName;
    private readonly bool _isConfigured;

    public AzureBlobStorageService(IConfiguration configuration, ILogger<AzureBlobStorageService> logger)
    {
        _logger = logger;
        var connectionString = configuration["AzureBlobStorage:ConnectionString"];
        _containerName = configuration["AzureBlobStorage:ContainerName"] ?? "hrm-employee-documents";

        if (!string.IsNullOrWhiteSpace(connectionString) && !connectionString.StartsWith("TODO", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                _blobServiceClient = new BlobServiceClient(connectionString);
                _containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                _containerClient.CreateIfNotExists(PublicAccessType.None);
                _isConfigured = true;
                _logger.LogInformation("Azure Blob Storage initialized successfully with container {ContainerName}.", _containerName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to initialize Azure Blob Storage with provided connection string. Storage calls will fall back to local disk storage if configured.");
                _isConfigured = false;
            }
        }
        else
        {
            _logger.LogWarning("AzureBlobStorage:ConnectionString is not configured or is a placeholder. Azure Blob Storage operations will use local simulated storage.");
            _isConfigured = false;
        }
    }

    public async Task<string> UploadBlobAsync(string blobPath, Stream contentStream, string contentType, CancellationToken cancellationToken = default)
    {
        // Normalize path
        var normalizedPath = blobPath.Replace('\\', '/').TrimStart('/');

        if (_isConfigured && _containerClient != null)
        {
            var blobClient = _containerClient.GetBlobClient(normalizedPath);
            var headers = new BlobHttpHeaders { ContentType = contentType };
            contentStream.Position = 0;
            await blobClient.UploadAsync(contentStream, new BlobUploadOptions { HttpHeaders = headers }, cancellationToken);
            return blobClient.Uri.ToString();
        }

        // Fallback to local storage under AppData/BlobStorage for local development/offline testing
        var localDir = Path.Combine(AppContext.BaseDirectory, "LocalStorageFallback", _containerName);
        var localFilePath = Path.Combine(localDir, normalizedPath.Replace('/', Path.DirectorySeparatorChar));
        var dir = Path.GetDirectoryName(localFilePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        contentStream.Position = 0;
        using (var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await contentStream.CopyToAsync(fileStream, cancellationToken);
        }

        _logger.LogInformation("Saved blob locally to fallback path: {Path}", localFilePath);
        return $"local://{normalizedPath}";
    }

    public async Task<Stream?> DownloadBlobAsync(string blobPath, CancellationToken cancellationToken = default)
    {
        var normalizedPath = blobPath.Replace('\\', '/').TrimStart('/');

        if (_isConfigured && _containerClient != null)
        {
            var blobClient = _containerClient.GetBlobClient(normalizedPath);
            if (!await blobClient.ExistsAsync(cancellationToken))
            {
                return null;
            }

            var downloadResponse = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
            return downloadResponse.Value.Content;
        }

        // Fallback to local storage
        var localDir = Path.Combine(AppContext.BaseDirectory, "LocalStorageFallback", _containerName);
        var localFilePath = Path.Combine(localDir, normalizedPath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(localFilePath))
        {
            return new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        return null;
    }

    public async Task<bool> DeleteBlobAsync(string blobPath, CancellationToken cancellationToken = default)
    {
        var normalizedPath = blobPath.Replace('\\', '/').TrimStart('/');

        if (_isConfigured && _containerClient != null)
        {
            var blobClient = _containerClient.GetBlobClient(normalizedPath);
            var result = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            return result.Value;
        }

        // Fallback to local storage
        var localDir = Path.Combine(AppContext.BaseDirectory, "LocalStorageFallback", _containerName);
        var localFilePath = Path.Combine(localDir, normalizedPath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(localFilePath))
        {
            File.Delete(localFilePath);
            return true;
        }

        return false;
    }

    public async Task<bool> BlobExistsAsync(string blobPath, CancellationToken cancellationToken = default)
    {
        var normalizedPath = blobPath.Replace('\\', '/').TrimStart('/');

        if (_isConfigured && _containerClient != null)
        {
            var blobClient = _containerClient.GetBlobClient(normalizedPath);
            return (await blobClient.ExistsAsync(cancellationToken)).Value;
        }

        var localDir = Path.Combine(AppContext.BaseDirectory, "LocalStorageFallback", _containerName);
        var localFilePath = Path.Combine(localDir, normalizedPath.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(localFilePath);
    }
}
