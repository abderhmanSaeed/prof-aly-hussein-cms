using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Infrastructure.Identity;

namespace ProfAly.CMS.Infrastructure.Persistence;

/// <summary>
/// EF Core context. In this skeleton it carries only the ASP.NET Core Identity
/// schema so authentication can be configured. The full v2.0 domain model
/// (TPH ContentItem, translation tables, the new entities, Redirect, etc.) and
/// the initial migration are added in Stage 2 — intentionally NOT here.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Stage 2 will configure: TPH discriminator, translation-table keys &
        // uniqueness, FK delete behaviours, indexes, culture CHECK constraints,
        // and apply IEntityTypeConfiguration<> mappings from this assembly.
    }
}
