using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Enums;

namespace ProfAly.CMS.Domain.Entities;

/// <summary>A scientific-society membership or editorial-board seat (doc 03 §2.4 / doc 05 §7).</summary>
public class Membership : AuditableEntity
{
    public MembershipKind Kind { get; set; }

    public int SortOrder { get; set; }

    public ICollection<MembershipTranslation> Translations { get; set; } = new List<MembershipTranslation>();
}

public class MembershipTranslation : BaseEntity, ITranslation
{
    public int MembershipId { get; set; }

    public Membership? Membership { get; set; }

    public string Culture { get; set; } = SupportedCultures.Default;

    public string Name { get; set; } = string.Empty;
}
