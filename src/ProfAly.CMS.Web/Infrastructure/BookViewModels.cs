namespace ProfAly.CMS.Web.Infrastructure;

/// <summary>
/// View model for the shared clickable book card (<c>_BookCard.cshtml</c>). Used by
/// both the Academic Books and Recommended Books listings (and the homepage book
/// rows) so the two sections share one card component (doc 82).
/// </summary>
public sealed class BookCardViewModel
{
    public required string Culture { get; init; }

    /// <summary>Razor page the card links to, e.g. <c>/Public/BookDetail</c>.</summary>
    public required string DetailPage { get; init; }

    public string? Slug { get; init; }

    /// <summary>Full URL of the cover image (e.g. <c>/uploads/...</c>), or null for the initial fallback.</summary>
    public string? CoverPath { get; init; }

    public string? Title { get; init; }

    /// <summary>Primary subtitle line (publisher or author).</summary>
    public string? Subtitle { get; init; }

    /// <summary>Optional secondary line (authorship role or year).</summary>
    public string? SubMeta { get; init; }

    public bool Featured { get; init; }

    public string Initial =>
        string.IsNullOrWhiteSpace(Title) ? "•" : Title.Trim()[..1];
}

/// <summary>
/// View model for the shared book details body (<c>_BookDetail.cshtml</c>). Drives the
/// cover + metadata + conditional action buttons shared by Academic Books and
/// Recommended Books (doc 82). Buttons render only when their field is populated.
/// </summary>
public sealed class BookDetailViewModel
{
    public string? CoverPath { get; init; }

    public string? Title { get; init; }

    public string? Author { get; init; }

    public string? Publisher { get; init; }

    public string? AuthorshipRole { get; init; }

    public int? Year { get; init; }

    public IReadOnlyList<string> Categories { get; init; } = Array.Empty<string>();

    public string? Summary { get; init; }

    public string? Body { get; init; }

    /// <summary>Full path of the attached PDF (e.g. <c>/uploads/...</c>), or null.</summary>
    public string? PdfPath { get; init; }

    /// <summary>Original filename used for the download action.</summary>
    public string? PdfName { get; init; }

    public string? ExternalUrl { get; init; }

    /// <summary>Storefront link ("Buy Book"); null for Academic Books.</summary>
    public string? PurchaseUrl { get; init; }

    public string Initial =>
        string.IsNullOrWhiteSpace(Title) ? "•" : Title.Trim()[..1];

    /// <summary>Resource key for the PDF-preview button label (lets each section keep its wording).</summary>
    public string PdfPreviewLabelKey { get; init; } = "Action_PreviewPdf";
}
