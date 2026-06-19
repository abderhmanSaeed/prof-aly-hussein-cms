using ProfAly.CMS.Domain.Common;

namespace ProfAly.CMS.Domain.Entities;

/// <summary>An academic qualification/degree of the professor (doc 03 §2.4 / doc 05 §5).</summary>
public class Qualification : AuditableEntity
{
    public int? Year { get; set; }

    public int SortOrder { get; set; }

    public ICollection<QualificationTranslation> Translations { get; set; } = new List<QualificationTranslation>();
}

public class QualificationTranslation : BaseEntity, ITranslation
{
    public int QualificationId { get; set; }

    public Qualification? Qualification { get; set; }

    public string Culture { get; set; } = SupportedCultures.Default;

    public string Degree { get; set; } = string.Empty;

    public string Institution { get; set; } = string.Empty;

    public string? Grade { get; set; }
}
