namespace HRAttendance.Business.Interfaces.Documents;

public interface IAzureBlobStorageService
{
    Task<string> UploadBlobAsync(string blobPath, Stream contentStream, string contentType, CancellationToken cancellationToken = default);
    Task<Stream?> DownloadBlobAsync(string blobPath, CancellationToken cancellationToken = default);
    Task<bool> DeleteBlobAsync(string blobPath, CancellationToken cancellationToken = default);
    Task<bool> BlobExistsAsync(string blobPath, CancellationToken cancellationToken = default);
}
