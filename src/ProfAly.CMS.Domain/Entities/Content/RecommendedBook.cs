using ProfAly.CMS.Domain.Enums;

namespace ProfAly.CMS.Domain.Entities.Content;

/// <summary>
/// A book recommended by Dr. Aly Hussein for reading (doc 76 — Digital Resources).
/// Distinct from the academic <see cref="Book"/> type: this is a reading
/// recommendation, optionally backed by a PDF, an external page, and/or a purchase
/// link. Author and publisher are translatable (on <see cref="ContentItemTranslation"/>,
/// reusing <c>Authors</c>/<c>Publisher</c>); cover image, PDF and external URL come
/// from the shared base.
/// </summary>
public class RecommendedBook : ContentItem
{
    public RecommendedBook() : base(ContentType.RecommendedBook)
    {
    }

    /// <summary>Optional storefront link shown as the "Buy Book" action (non-translatable).</summary>
    public string? PurchaseUrl { get; set; }
}
