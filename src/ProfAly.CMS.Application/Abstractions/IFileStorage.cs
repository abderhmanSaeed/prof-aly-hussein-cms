namespace ProfAly.CMS.Application.Abstractions;

/// <summary>
/// Storage-provider abstraction (doc 02 §4, doc 09). The filesystem implementation
/// lives in Infrastructure; callers depend only on this interface so storage can
/// later move to object storage (R2/B2) without code changes.
/// Validation, thumbnailing, and DB metadata (MediaFile) are the media service's
/// responsibility (Stage 5) — this layer only moves bytes.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Persists a stream under a date-partitioned, GUID-named path and returns its metadata.
    /// </summary>
    Task<StoredFile> SaveAsync(
        Stream content,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>Opens a stored file for reading, or returns null if it does not exist.</summary>
    Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default);

    /// <summary>Deletes a stored file. No-op if it does not exist.</summary>
    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);

    /// <summary>Returns true if a file exists at the given storage-relative path.</summary>
    bool Exists(string relativePath);
}

/// <summary>Result of a successful store operation. Paths are storage-relative (POSIX separators).</summary>
public sealed record StoredFile(
    string StoredFileName,
    string RelativePath,
    long SizeBytes,
    string ContentType);
