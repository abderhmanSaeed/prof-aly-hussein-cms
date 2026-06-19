using ProfAly.CMS.Domain.Common;

namespace ProfAly.CMS.Domain.Entities;

/// <summary>A "career in numbers" counter shown on the homepage (doc 03 §2.4 / doc 05 §8).</summary>
public class StatItem : AuditableEntity
{
    public int Value { get; set; }

    /// <summary>Optional suffix appended to the value, e.g. "+".</summary>
    public string? Suffix { get; set; }

    public int SortOrder { get; set; }

    public ICollection<StatItemTranslation> Translations { get; set; } = new List<StatItemTranslation>();
}

public class StatItemTranslation : BaseEntity, ITranslation
{
    public int StatItemId { get; set; }

    public StatItem? StatItem { get; set; }

    public string Culture { get; set; } = SupportedCultures.Default;

    public string Label { get; set; } = string.Empty;
}
