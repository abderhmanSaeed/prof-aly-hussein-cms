using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProfAly.CMS.Infrastructure;
using ProfAly.CMS.Infrastructure.Identity;
using ProfAly.CMS.Infrastructure.Persistence.Seeding;

namespace ProfAly.CMS.Tests;

/// <summary>
/// Verifies the administrator initialization flow the way Production runs it: the real
/// <see cref="DatabaseInitializer"/> + seeders wired through <c>AddInfrastructure</c>,
/// against a throwaway temp SQLite file, with the password supplied ONLY through
/// configuration (as appsettings.Production.json / the AdminAccount__Password env var do —
/// User Secrets are never loaded outside Development).
/// </summary>
public class AdminAccountSeedingTests : IDisposable
{
    private const string Email = "admin@aly-hussein.local";
    private const string Password = "Admin#2026Dev";

    private readonly string _dir;
    private readonly string _dbPath;

    public AdminAccountSeedingTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "profaly-admin-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
        _dbPath = Path.Combine(_dir, "app.db");
    }

    private ServiceProvider BuildProvider(string? adminPassword)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_dbPath}",
                ["AdminAccount:Email"] = Email,
                ["AdminAccount:Password"] = adminPassword,
                ["Seed:ImportStaticContent"] = "false",
                ["Backup:Enabled"] = "false", // keep the test hermetic (no backup files)
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config); // the web host registers this automatically
        services.AddLogging();
        services.AddInfrastructure(config);
        return services.BuildServiceProvider();
    }

    private static async Task RunStartupAsync(ServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().RunAsync();
    }

    [Fact]
    public async Task FreshProductionStartup_CreatesAdmin_FromConfig_AndLoginPasswordValidates()
    {
        // Empty database + password only in configuration = the clean-VPS scenario.
        using var sp = BuildProvider(Password);
        await RunStartupAsync(sp);

        using var scope = sp.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var admin = await users.FindByEmailAsync(Email);
        Assert.NotNull(admin);
        Assert.True(admin!.EmailConfirmed);
        // Identity password hashing round-trips → login with these credentials succeeds.
        Assert.True(await users.CheckPasswordAsync(admin, Password));
        Assert.True(await users.IsInRoleAsync(admin, Roles.SuperAdmin));
    }

    [Fact]
    public async Task SecondStartup_DoesNotDuplicate_Reset_OrOverwrite_TheExistingAdmin()
    {
        // First boot creates the admin.
        using var sp1 = BuildProvider(Password);
        await RunStartupAsync(sp1);

        string originalHash;
        using (var scope = sp1.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            originalHash = (await users.FindByEmailAsync(Email))!.PasswordHash!;
        }

        // A later restart — even if the configured password CHANGED — must not touch the
        // existing account (no duplicate, no hash regeneration, no password reset).
        SqliteConnection.ClearAllPools();
        using var sp2 = BuildProvider("Totally#Different#2027");
        await RunStartupAsync(sp2);

        using var scope2 = sp2.CreateScope();
        var users2 = scope2.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        Assert.Single(users2.Users.ToList());                                   // no duplicate admin
        var admin = await users2.FindByEmailAsync(Email);
        Assert.Equal(originalHash, admin!.PasswordHash);                        // hash NOT regenerated
        Assert.True(await users2.CheckPasswordAsync(admin, Password));          // original password still works
        Assert.False(await users2.CheckPasswordAsync(admin, "Totally#Different#2027")); // config change ignored
    }

    [Fact]
    public async Task WithoutAnyConfiguredPassword_AdminIsSkipped_NotCreated()
    {
        // Documents the pre-fix failure mode: no password in any Production config source
        // (User Secrets absent) → seeding is skipped and there is NO admin to log in as.
        using var sp = BuildProvider(adminPassword: null);
        await RunStartupAsync(sp);

        using var scope = sp.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.Null(await users.FindByEmailAsync(Email));
    }

    [Fact]
    public void IdentitySecurity_LockoutRolesAndHashing_AreConfigured()
    {
        using var sp = BuildProvider(Password);
        using var scope = sp.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<IdentityOptions>>().Value;

        // Lockout (brute-force protection) unchanged.
        Assert.Equal(5, options.Lockout.MaxFailedAccessAttempts);
        Assert.Equal(TimeSpan.FromMinutes(15), options.Lockout.DefaultLockoutTimeSpan);
        Assert.True(options.Lockout.AllowedForNewUsers);

        // Strong password policy unchanged.
        Assert.Equal(10, options.Password.RequiredLength);
        Assert.True(options.Password.RequireDigit);
        Assert.True(options.Password.RequireUppercase);
        Assert.True(options.Password.RequireNonAlphanumeric);

        // A real password hasher is registered (Identity default: PBKDF2).
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>());
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { Directory.Delete(_dir, recursive: true); } catch { /* temp cleanup best-effort */ }
        GC.SuppressFinalize(this);
    }
}
