using ProfAly.CMS.Domain.Common;

namespace ProfAly.CMS.Domain.Entities;

/// <summary>
/// A group of professional activities (e.g. "Training & Quality Assurance"),
/// doc 03 §2.4 / doc 05 §10. Contains ordered <see cref="Activity"/> items.
/// </summary>
public class ActivityGroup : AuditableEntity
{
    public int SortOrder { get; set; }

    public ICollection<ActivityGroupTranslation> Translations { get; set; } = new List<ActivityGroupTranslation>();

    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}

public class ActivityGroupTranslation : BaseEntity, ITranslation
{
    public int ActivityGroupId { get; set; }

    public ActivityGroup? ActivityGroup { get; set; }

    public string Culture { get; set; } = SupportedCultures.Default;

    public string Name { get; set; } = string.Empty;
}
