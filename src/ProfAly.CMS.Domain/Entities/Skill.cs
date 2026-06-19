using ProfAly.CMS.Domain.Common;

namespace ProfAly.CMS.Domain.Entities;

/// <summary>A competency/skill chip (doc 03 §2.4 / doc 05 §6).</summary>
public class Skill : AuditableEntity
{
    public int SortOrder { get; set; }

    public ICollection<SkillTranslation> Translations { get; set; } = new List<SkillTranslation>();
}

public class SkillTranslation : BaseEntity, ITranslation
{
    public int SkillId { get; set; }

    public Skill? Skill { get; set; }

    public string Culture { get; set; } = SupportedCultures.Default;

    public string Name { get; set; } = string.Empty;
}
