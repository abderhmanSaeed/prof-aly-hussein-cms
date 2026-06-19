namespace ProfAly.CMS.Infrastructure.Identity;

/// <summary>
/// Authorization constants for the single-admin model (doc 06 §7).
/// One role and one policy gate the entire /admin area.
/// </summary>
public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
}

public static class Policies
{
    public const string RequireSuperAdmin = "RequireSuperAdmin";
}
