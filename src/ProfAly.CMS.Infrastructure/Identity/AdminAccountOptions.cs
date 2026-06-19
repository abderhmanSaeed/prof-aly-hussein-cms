namespace ProfAly.CMS.Infrastructure.Identity;

/// <summary>
/// Bound from the "AdminAccount" configuration section. The <see cref="Password"/>
/// is NEVER stored in appsettings — supply it via environment variable
/// (<c>AdminAccount__Password</c>) or user-secrets. If absent, the seeder skips
/// admin creation with a warning rather than using a hardcoded default.
/// </summary>
public sealed class AdminAccountOptions
{
    public const string SectionName = "AdminAccount";

    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }
}
