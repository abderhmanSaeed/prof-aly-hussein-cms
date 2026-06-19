namespace ProfAly.CMS.Domain.Common;

/// <summary>Base for all persisted entities: an integer surrogate key (doc 03 §4).</summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
}
