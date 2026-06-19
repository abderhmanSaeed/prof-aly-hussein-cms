using ProfAly.CMS.Domain.Common;

namespace ProfAly.CMS.Domain.Entities;

/// <summary>A message submitted via the public contact form (doc 03 §2.7 / doc 05 §15).</summary>
public class ContactMessage : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Subject { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    /// <summary>Captured server-side for abuse triage.</summary>
    public string? IpAddress { get; set; }

    public DateTime CreatedUtc { get; set; }
}
