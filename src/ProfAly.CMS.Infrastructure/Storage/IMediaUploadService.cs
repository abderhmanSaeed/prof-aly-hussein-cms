using Microsoft.AspNetCore.Http;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Domain.Enums;

namespace ProfAly.CMS.Infrastructure.Storage;

/// <summary>
/// Validates an uploaded file (extension + size + content sniffing, doc 09), stores it
/// via <c>IFileStorage</c>, and persists a <see cref="MediaFile"/> metadata row.
/// Returns a result with a resource key on failure so callers can localize the message.
/// </summary>
public interface IMediaUploadService
{
    Task<MediaUploadResult> UploadAsync(
        IFormFile file,
        MediaKind kind,
        string? altText = null,
        CancellationToken cancellationToken = default);
}

public sealed record MediaUploadResult(bool Succeeded, MediaFile? File, string? ErrorKey)
{
    public static MediaUploadResult Ok(MediaFile file) => new(true, file, null);

    public static MediaUploadResult Fail(string errorKey) => new(false, null, errorKey);
}
