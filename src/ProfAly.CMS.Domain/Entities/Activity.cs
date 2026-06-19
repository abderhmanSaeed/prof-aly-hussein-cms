using ProfAly.CMS.Domain.Common;

namespace ProfAly.CMS.Domain.Entities;

/// <summary>A single professional-activity line within an <see cref="ActivityGroup"/> (doc 05 §10).</summary>
public class Activity : AuditableEntity
{
    public int ActivityGroupId { get; set; }

    public ActivityGroup? ActivityGroup { get; set; }

    public int SortOrder { get; set; }

    public ICollection<ActivityTranslation> Translations { get; set; } = new List<ActivityTranslation>();
}

public class ActivityTranslation : BaseEntity, ITranslation
{
    public int ActivityId { get; set; }

    public Activity? Activity { get; set; }

    public string Culture { get; set; } = SupportedCultures.Default;

    public string Text { get; set; } = string.Empty;
}
