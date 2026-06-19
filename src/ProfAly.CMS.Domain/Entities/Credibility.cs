using ProfAly.CMS.Domain.Common;

namespace ProfAly.CMS.Domain.Entities;

/// <summary>An institution/credibility chip on the homepage (doc 03 §2.4 / doc 05 §9).</summary>
public class Credibility : AuditableEntity
{
    public int? LogoMediaId { get; set; }

    public MediaFile? Logo { get; set; }

    public int SortOrder { get; set; }

    public ICollection<CredibilityTranslation> Translations { get; set; } = new List<CredibilityTranslation>();
}

public class CredibilityTranslation : BaseEntity, ITranslation
{
    public int CredibilityId { get; set; }

    public Credibility? Credibility { get; set; }

    public string Culture { get; set; } = SupportedCultures.Default;

    public string Name { get; set; } = string.Empty;
}
