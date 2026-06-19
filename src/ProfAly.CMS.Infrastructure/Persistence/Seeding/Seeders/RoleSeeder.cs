using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ProfAly.CMS.Infrastructure.Identity;

namespace ProfAly.CMS.Infrastructure.Persistence.Seeding.Seeders;

/// <summary>Ensures the single <c>SuperAdmin</c> role exists (doc 06 §7).</summary>
public sealed class RoleSeeder : IDataSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<RoleSeeder> _logger;

    public RoleSeeder(RoleManager<IdentityRole> roleManager, ILogger<RoleSeeder> logger)
    {
        _roleManager = roleManager;
        _logger = logger;
    }

    public int Order => 1;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _roleManager.RoleExistsAsync(Roles.SuperAdmin))
        {
            _logger.LogInformation("Role '{Role}' already exists.", Roles.SuperAdmin);
            return;
        }

        var result = await _roleManager.CreateAsync(new IdentityRole(Roles.SuperAdmin));
        if (result.Succeeded)
        {
            _logger.LogInformation("Created role '{Role}'.", Roles.SuperAdmin);
        }
        else
        {
            _logger.LogError("Failed to create role '{Role}': {Errors}", Roles.SuperAdmin,
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }
}
