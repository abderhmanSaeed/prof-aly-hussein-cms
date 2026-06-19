using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Enums;

namespace ProfAly.CMS.Domain.Entities;

/// <summary>A taught course (doc 03 §2.5 / doc 05 §13), grouped by <see cref="Level"/>.</summary>
public class Course : AuditableEntity
{
    public CourseLevel Level { get; set; }

    /// <summary>Optional period label, e.g. "2023–2024".</summary>
    public string? Period { get; set; }

    public int SortOrder { get; set; }

    public ICollection<CourseTranslation> Translations { get; set; } = new List<CourseTranslation>();
}

public class CourseTranslation : BaseEntity, ITranslation
{
    public int CourseId { get; set; }

    public Course? Course { get; set; }

    public string Culture { get; set; } = SupportedCultures.Default;

    public string CourseName { get; set; } = string.Empty;

    public string? Institution { get; set; }

    public string? Description { get; set; }
}
