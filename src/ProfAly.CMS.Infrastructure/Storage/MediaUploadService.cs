using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ProfAly.CMS.Application.Abstractions;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Domain.Enums;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Infrastructure.Storage;

/// <summary>
/// Default <see cref="IMediaUploadService"/>: allowlist + size + magic-byte validation,
/// storage via <see cref="IFileStorage"/>, and a persisted <see cref="MediaFile"/> row.
/// SVG/HTML/executables are rejected (no matching magic bytes).
/// </summary>
public sealed class MediaUploadService : IMediaUploadService
{
    private readonly IFileStorage _storage;
    private readonly AppDbContext _db;
    private readonly FileStorageOptions _options;

    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly string[] PdfExtensions = { ".pdf" };

    // Enrichment documents: PDF plus Microsoft Word and PowerPoint formats.
    private static readonly string[] DocumentExtensions = { ".pdf", ".doc", ".docx", ".ppt", ".pptx" };

    public MediaUploadService(IFileStorage storage, AppDbContext db, IOptions<FileStorageOptions> options)
    {
        _storage = storage;
        _db = db;
        _options = options.Value;
    }

    public async Task<MediaUploadResult> UploadAsync(
        IFormFile file,
        MediaKind kind,
        string? altText = null,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return MediaUploadResult.Fail("Upload_Empty");
        }

        var (allowedExtensions, maxBytes, invalidTypeKey) = kind switch
        {
            MediaKind.Pdf => (PdfExtensions, _options.MaxPdfBytes, "Upload_InvalidType_Pdf"),
            MediaKind.Document => (DocumentExtensions, _options.MaxDocumentBytes, "Upload_InvalidType_Document"),
            _ => (ImageExtensions, _options.MaxImageBytes, "Upload_InvalidType_Image"),
        };

        if (file.Length > maxBytes)
        {
            return MediaUploadResult.Fail("Upload_TooLarge");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return MediaUploadResult.Fail(invalidTypeKey);
        }

        await using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;

        var header = new byte[12];
        var read = await buffer.ReadAsync(header.AsMemory(0, 12), cancellationToken);
        buffer.Position = 0;

        if (!MatchesMagic(header, read, extension))
        {
            return MediaUploadResult.Fail(invalidTypeKey);
        }

        var contentType = ContentTypeFor(extension);
        var stored = await _storage.SaveAsync(buffer, file.FileName, contentType, cancellationToken);

        var media = new MediaFile
        {
            StoredFileName = stored.StoredFileName,
            OriginalFileName = Sanitize(file.FileName),
            RelativePath = stored.RelativePath,
            ContentType = contentType,
            MediaKind = kind,
            SizeBytes = stored.SizeBytes,
            AltText = altText,
            CreatedUtc = DateTime.UtcNow,
        };

        _db.MediaFile.Add(media);
        await _db.SaveChangesAsync(cancellationToken);

        return MediaUploadResult.Ok(media);
    }

    private static bool MatchesMagic(byte[] h, int read, string extension)
    {
        if (read < 4)
        {
            return false;
        }

        return extension switch
        {
            ".jpg" or ".jpeg" => h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF,
            ".png" => h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47,
            ".webp" => read >= 12 && h[0] == (byte)'R' && h[1] == (byte)'I' && h[2] == (byte)'F' && h[3] == (byte)'F'
                       && h[8] == (byte)'W' && h[9] == (byte)'E' && h[10] == (byte)'B' && h[11] == (byte)'P',
            ".pdf" => h[0] == 0x25 && h[1] == 0x50 && h[2] == 0x44 && h[3] == 0x46,
            // Office Open XML (.docx/.pptx) are ZIP containers: "PK\x03\x04".
            ".docx" or ".pptx" => h[0] == 0x50 && h[1] == 0x4B && h[2] == 0x03 && h[3] == 0x04,
            // Legacy Office (.doc/.ppt) are OLE2 compound files: D0 CF 11 E0 (A1 B1 1A E1).
            ".doc" or ".ppt" => h[0] == 0xD0 && h[1] == 0xCF && h[2] == 0x11 && h[3] == 0xE0,
            _ => false,
        };
    }

    private static string ContentTypeFor(string extension) => extension switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".pdf" => "application/pdf",
        ".doc" => "application/msword",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".ppt" => "application/vnd.ms-powerpoint",
        ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        _ => "application/octet-stream",
    };

    private static string Sanitize(string fileName)
    {
        var name = Path.GetFileName(fileName);
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }

        return name.Length > 260 ? name[^260..] : name;
    }
}
