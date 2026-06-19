using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProfAly.CMS.Application.Abstractions;

namespace ProfAly.CMS.Infrastructure.Storage;

/// <summary>
/// Filesystem implementation of <see cref="IFileStorage"/> (doc 09): date-partitioned
/// directories, GUID stored names, original name preserved only in DB metadata.
/// Behind the abstraction so it can be swapped for object storage later.
/// </summary>
public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _root;
    private readonly ILogger<LocalFileStorage> _logger;

    public LocalFileStorage(IOptions<FileStorageOptions> options, ILogger<LocalFileStorage> logger)
    {
        _logger = logger;
        _root = Path.GetFullPath(options.Value.RootPath);
        Directory.CreateDirectory(_root);
    }

    public async Task<StoredFile> SaveAsync(
        Stream content,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var extension = Path.GetExtension(originalFileName)?.ToLowerInvariant() ?? string.Empty;
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var relativeDir = $"{now:yyyy}/{now:MM}";
        var relativePath = $"{relativeDir}/{storedName}";

        var absoluteDir = Path.Combine(_root, relativeDir.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(absoluteDir);

        var absolutePath = Path.Combine(absoluteDir, storedName);
        await using (var target = new FileStream(absolutePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await content.CopyToAsync(target, cancellationToken);
        }

        var size = new FileInfo(absolutePath).Length;
        _logger.LogInformation("Stored file {RelativePath} ({Size} bytes)", relativePath, size);
        return new StoredFile(storedName, relativePath, size, contentType);
    }

    public Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var absolutePath = ResolveWithinRoot(relativePath);
        if (absolutePath is null || !File.Exists(absolutePath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var absolutePath = ResolveWithinRoot(relativePath);
        if (absolutePath is not null && File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
            _logger.LogInformation("Deleted file {RelativePath}", relativePath);
        }

        return Task.CompletedTask;
    }

    public bool Exists(string relativePath)
    {
        var absolutePath = ResolveWithinRoot(relativePath);
        return absolutePath is not null && File.Exists(absolutePath);
    }

    /// <summary>Resolves a storage-relative path to an absolute path, rejecting traversal outside the root.</summary>
    private string? ResolveWithinRoot(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
        var absolutePath = Path.GetFullPath(Path.Combine(_root, normalized));

        var rootWithSeparator = _root.EndsWith(Path.DirectorySeparatorChar)
            ? _root
            : _root + Path.DirectorySeparatorChar;

        return absolutePath.StartsWith(rootWithSeparator, StringComparison.Ordinal) ? absolutePath : null;
    }
}
