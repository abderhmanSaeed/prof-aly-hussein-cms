using ProfAly.CMS.Domain.Common;

namespace ProfAly.CMS.Domain.Entities;

/// <summary>
/// A career/position entry on the experience timeline (doc 03 §2.5 / doc 05 §12).
/// Dates are optional structured metadata; <see cref="ExperienceEntryTranslation.PeriodLabel"/>
/// preserves the exact wording (e.g. "Jul 2018 – present"). Ordering is by <see cref="SortOrder"/>.
/// </summary>
public class ExperienceEntry : AuditableEntity, IValidatableEntity
{
    public DateTime? StartDateUtc { get; set; }

    /// <summary>Null = present.</summary>
    public DateTime? EndDateUtc { get; set; }

    public int SortOrder { get; set; }

    public ICollection<ExperienceEntryTranslation> Translations { get; set; } = new List<ExperienceEntryTranslation>();

    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (StartDateUtc.HasValue && EndDateUtc.HasValue && EndDateUtc.Value < StartDateUtc.Value)
        {
            errors.Add("End date must be on or after the start date.");
        }

        return errors;
    }
}

public class ExperienceEntryTranslation : BaseEntity, ITranslation
{
    public int ExperienceEntryId { get; set; }

    public ExperienceEntry? ExperienceEntry { get; set; }

    public string Culture { get; set; } = SupportedCultures.Default;

    public string Role { get; set; } = string.Empty;

    public string Organization { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>Free-text period display, e.g. "Jul 2018 – present".</summary>
    public string? PeriodLabel { get; set; }
}
