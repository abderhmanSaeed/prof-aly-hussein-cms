namespace ProfAly.CMS.Domain.Common;

/// <summary>
/// Base for entities that carry creation/modification timestamps (UTC).
/// <c>CreatedUtc</c> is set on insert; <c>ModifiedUtc</c> on every update (doc 05).
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedUtc { get; set; }

    public DateTime ModifiedUtc { get; set; }
}
