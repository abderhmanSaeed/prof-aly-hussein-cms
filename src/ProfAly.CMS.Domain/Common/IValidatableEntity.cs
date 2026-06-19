namespace ProfAly.CMS.Domain.Common;

/// <summary>
/// Implemented by entities that carry cross-field domain invariants
/// (e.g. date-range ordering, conditional-required rules). Returns human-readable
/// violation messages; an empty result means the invariants hold. Services call
/// this before persistence; it is independent of EF and data-annotation checks.
/// </summary>
public interface IValidatableEntity
{
    IReadOnlyList<string> Validate();
}
