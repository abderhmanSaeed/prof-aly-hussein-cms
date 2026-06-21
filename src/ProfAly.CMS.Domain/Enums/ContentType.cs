namespace ProfAly.CMS.Domain.Enums;

/// <summary>
/// Discriminator for the Table-Per-Hierarchy content model (doc 03 §1.1).
/// Each value corresponds to a concrete <c>ContentItem</c> subtype.
/// </summary>
public enum ContentType
{
    Book = 1,
    Publication = 2,
    ResearchPaper = 3,
    Thesis = 4,
    Project = 5,
    Resource = 6,
    EnrichmentItem = 7,
    Video = 8,
    RecommendedBook = 9,
    Event = 10,
}
