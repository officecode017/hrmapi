using Microsoft.AspNetCore.Http;

namespace HRAttendance.Business.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string subfolder, CancellationToken cancellationToken = default);
    bool DeleteFile(string? relativePath);
}
