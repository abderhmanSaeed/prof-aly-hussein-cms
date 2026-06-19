namespace ProfAly.CMS.Domain.Enums;

/// <summary>
/// The professor's relationship to a thesis (doc 05 §3). Replaces the removed
/// <c>Supervisor</c> field — the professor is always the supervisor/examiner.
/// </summary>
public enum RelationshipType
{
    Supervised = 1,
    Examined = 2,
    Ongoing = 3,
}
