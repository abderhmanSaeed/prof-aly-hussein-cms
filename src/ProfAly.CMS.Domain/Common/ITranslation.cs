namespace ProfAly.CMS.Domain.Common;

/// <summary>
/// Marker for per-culture translation rows keyed by (ParentId, Culture).
/// <c>Culture</c> is constrained to the supported set (doc 03 §1.2 / §4).
/// </summary>
public interface ITranslation
{
    string Culture { get; set; }
}
