using ProfAly.CMS.Domain.Enums;

namespace ProfAly.CMS.Domain.Entities.Content;

/// <summary>
/// A supervised/examined thesis by another researcher (doc 08 §8). The researcher's
/// name is translatable (on the translation row); the professor's relationship to it
/// is captured by <see cref="RelationshipType"/> — replacing the removed Supervisor field.
/// </summary>
public class Thesis : ContentItem
{
    public Thesis() : base(ContentType.Thesis)
    {
    }

    public DegreeLevel DegreeLevel { get; set; }

    public RelationshipType RelationshipType { get; set; }
}
