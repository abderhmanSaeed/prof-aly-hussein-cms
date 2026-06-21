using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Enums;

namespace ProfAly.CMS.Domain.Entities.Content;

/// <summary>
/// Abstract base of the Table-Per-Hierarchy content model (doc 03 §1.1). Shared,
/// non-translatable fields live here; type-specific scalar fields live on the
/// subtypes (EF flattens them into one table). Translatable text — including
/// type-specific text such as Journal/Authors/Publisher/ResearcherName — lives on
/// <see cref="ContentItemTranslation"/>.
/// </summary>
public abstract class ContentItem : AuditableEntity, IValidatableEntity
{
    /// <summary>Each subtype fixes its content type, which is also the persisted discriminator.</summary>
    protected ContentItem(ContentType contentType) => ContentType = contentType;

    /// <summary>
    /// The content type. Doubles as the TPH discriminator (column "ContentType", stored
    /// as TEXT). Set by the subtype constructor and managed by EF during materialization.
    /// </summary>
    public ContentType ContentType { get; private set; }

    public int? CoverImageId { get; set; }

    public MediaFile? CoverImage { get; set; }

    public int? PdfFileId { get; set; }

    public MediaFile? PdfFile { get; set; }

    /// <summary>External link (e.g. project page, DOI landing).</summary>
    public string? ExternalUrl { get; set; }

    public int? PublicationYear { get; set; }

    /// <summary>Relevant date for projects/theses.</summary>
    public DateTime? EventDateUtc { get; set; }

    public bool IsPublished { get; set; }

    /// <summary>Homepage featuring flag (primarily Books). doc 03 §2.2.</summary>
    public bool IsFeatured { get; set; }

    public int SortOrder { get; set; }

    public int ViewCount { get; set; }

    public int DownloadCount { get; set; }

    public ICollection<ContentItemTranslation> Translations { get; set; } = new List<ContentItemTranslation>();

    public ICollection<ContentItemCategory> Categories { get; set; } = new List<ContentItemCategory>();

    public ICollection<ContentEvent> Events { get; set; } = new List<ContentEvent>();

    /// <summary>Ordered gallery images (in addition to <see cref="CoverImage"/>). Used by Events. doc 76.</summary>
    public ICollection<ContentImage> Images { get; set; } = new List<ContentImage>();

    /// <summary>
    /// Base invariants: publication year (if present) must be within range. Subtypes
    /// override to add their own rules and should combine results with the base set.
    /// </summary>
    public virtual IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (PublicationYear.HasValue && !ContentRules.IsValidPublicationYear(PublicationYear.Value))
        {
            errors.Add($"Publication year must be between {ContentRules.MinPublicationYear} and {ContentRules.MaxPublicationYear()}.");
        }

        return errors;
    }
}
