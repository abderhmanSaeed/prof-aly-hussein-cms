using ProfAly.CMS.Domain.Common;

namespace ProfAly.CMS.Domain.Entities;

/// <summary>
/// A URL alias / 301-302 redirect (doc 03 §2.8 / doc 05 §16). Serves the legacy
/// "*.html" cutover and slug-change redirects; resolved by middleware before routing.
/// </summary>
public class Redirect : AuditableEntity, IValidatableEntity
{
    /// <summary>Source path, unique, e.g. "/about.html".</summary>
    public string FromPath { get; set; } = string.Empty;

    /// <summary>Target path, e.g. "/ar/about".</summary>
    public string ToPath { get; set; } = string.Empty;

    public int StatusCode { get; set; } = 301;

    public bool IsActive { get; set; } = true;

    public string? Notes { get; set; }

    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (!ContentRules.IsValidRedirectStatusCode(StatusCode))
        {
            errors.Add("Redirect status code must be 301 or 302.");
        }

        if (!string.IsNullOrWhiteSpace(FromPath) &&
            string.Equals(FromPath, ToPath, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Redirect source and target paths must differ.");
        }

        return errors;
    }
}
