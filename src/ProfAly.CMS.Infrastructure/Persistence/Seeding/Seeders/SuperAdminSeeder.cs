using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProfAly.CMS.Infrastructure.Identity;

namespace ProfAly.CMS.Infrastructure.Persistence.Seeding.Seeders;

/// <summary>
/// Seeds the single Super Admin account from configuration (doc 06 §7). The password
/// is read from <see cref="AdminAccountOptions"/> (env var / user-secrets) — never
/// hardcoded. If email or password is not configured, creation is skipped with a
/// warning so the app still starts.
/// </summary>
public sealed class SuperAdminSeeder : IDataSeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AdminAccountOptions _options;
    private readonly ILogger<SuperAdminSeeder> _logger;

    public SuperAdminSeeder(
        UserManager<ApplicationUser> userManager,
        IOptions<AdminAccountOptions> options,
        ILogger<SuperAdminSeeder> logger)
    {
        _userManager = userManager;
        _options = options.Value;
        _logger = logger;
    }

    public int Order => 2;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Email))
        {
            _logger.LogWarning("AdminAccount:Email is not configured; skipping Super Admin seeding.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.Password))
        {
            _logger.LogWarning(
                "AdminAccount:Password is not configured; skipping Super Admin seeding. " +
                "Set it via the AdminAccount__Password environment variable or user-secrets.");
            return;
        }

        var existing = await _userManager.FindByEmailAsync(_options.Email);
        if (existing is not null)
        {
            await EnsureInRoleAsync(existing);
            _logger.LogInformation("Super Admin '{Email}' already exists.", _options.Email);
            return;
        }

        var user = new ApplicationUser
        {
            UserName = _options.Email,
            Email = _options.Email,
            EmailConfirmed = true,
        };

        var create = await _userManager.CreateAsync(user, _options.Password);
        if (!create.Succeeded)
        {
            _logger.LogError("Failed to create Super Admin '{Email}': {Errors}", _options.Email,
                string.Join("; ", create.Errors.Select(e => e.Description)));
            return;
        }

        await EnsureInRoleAsync(user);
        _logger.LogInformation("Created Super Admin '{Email}'.", _options.Email);
    }

    private async Task EnsureInRoleAsync(ApplicationUser user)
    {
        if (!await _userManager.IsInRoleAsync(user, Roles.SuperAdmin))
        {
            await _userManager.AddToRoleAsync(user, Roles.SuperAdmin);
        }
    }
}
