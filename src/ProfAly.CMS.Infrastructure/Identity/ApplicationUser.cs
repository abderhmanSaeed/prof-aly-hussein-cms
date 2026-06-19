using Microsoft.AspNetCore.Identity;

namespace ProfAly.CMS.Infrastructure.Identity;

/// <summary>
/// The single Super Admin principal. Extends IdentityUser so profile-specific
/// admin fields can be added later without touching the auth model.
/// </summary>
public class ApplicationUser : IdentityUser
{
}
