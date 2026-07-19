using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProfAly.CMS.Infrastructure;
using ProfAly.CMS.Infrastructure.Identity;
using ProfAly.CMS.Infrastructure.Persistence.Seeding;

namespace ProfAly.CMS.Tests;

/// <summary>
/// Verifies the administrator initialization flow the way Production runs it: the real
/// <see cref="DatabaseInitializer"/> + seeders wired through <c>AddInfrastructure</c>,
/// against a throwaway temp SQLite file. The bootstrap password is supplied ONLY through the
/// <c>AdminAccount__Password</c> environment variable (as the server-side EnvironmentFile does) —
/// never from a source-controlled file. User Secrets are not loaded outside Development.
/// </summary>
public class AdminAccountSeedingTests : IDisposable
{
    private const string Email = "admin@aly-hussein.local";
    private const string Password = "Admin#2026Dev";
    private const string EnvVarName = "AdminAccount__Password"; // "__" binds to AdminAccount:Password

    private readonly string _dir;
    private readonly string _dbPath;

    public AdminAccountSeedingTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "profaly-admin-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
        _dbPath = Path.Combine(_dir, "app.db");
        // Start from a known-clean state so a stray env var can never leak into a test.
        Environment.SetEnvironmentVariable(EnvVarName, null);
    }

    /// <summary>Sets (or clears) the bootstrap-password environment variable for the next build.</summary>
    private static void SetPasswordEnv(string? value) =>
        Environment.SetEnvironmentVariable(EnvVarName, value);

    /// <summary>
    /// Builds the provider the way the web host does: email comes from configuration
    /// (appsettings), the password comes ONLY from environment variables via
    /// <see cref="ConfigurationBuilder.AddEnvironmentVariables()"/>.
    /// </summary>
    private ServiceProvider BuildProvider(CapturingLoggerProvider? logCapture = null)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_dbPath}",
                ["AdminAccount:Email"] = Email, // password intentionally NOT set here
                ["Seed:ImportStaticContent"] = "false",
                ["Backup:Enabled"] = "false", // keep the test hermetic (no backup files)
            })
            .AddEnvironmentVariables() // AdminAccount__Password is read from here, if present
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config); // the web host registers this automatically
        services.AddLogging(b =>
        {
            if (logCapture is not null)
            {
                b.AddProvider(logCapture);
            }
        });
        services.AddInfrastructure(config);
        return services.BuildServiceProvider();
    }

    private static async Task RunStartupAsync(ServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().RunAsync();
    }

    [Fact]
    public async Task FreshProductionStartup_CreatesAdmin_FromEnvironmentVariable_AndLoginPasswordValidates()
    {
        // Empty database + password ONLY in the environment variable = the clean-VPS scenario.
        SetPasswordEnv(Password);

        using var sp = BuildProvider();
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
        // First boot creates the admin from the env var.
        SetPasswordEnv(Password);
        using var sp1 = BuildProvider();
        await RunStartupAsync(sp1);

        string originalHash;
        using (var scope = sp1.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            originalHash = (await users.FindByEmailAsync(Email))!.PasswordHash!;
        }

        // A later restart — even if the env-var password CHANGED — must not touch the existing
        // account (no duplicate, no hash regeneration, no password reset).
        SqliteConnection.ClearAllPools();
        SetPasswordEnv("Totally#Different#2027");
        using var sp2 = BuildProvider();
        await RunStartupAsync(sp2);

        using var scope2 = sp2.CreateScope();
        var users2 = scope2.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        Assert.Single(users2.Users.ToList());                                   // no duplicate admin
        var admin = await users2.FindByEmailAsync(Email);
        Assert.Equal(originalHash, admin!.PasswordHash);                        // hash NOT regenerated
        Assert.True(await users2.CheckPasswordAsync(admin, Password));          // original password still works
        Assert.False(await users2.CheckPasswordAsync(admin, "Totally#Different#2027")); // env change ignored
    }

    [Fact]
    public async Task WithoutEnvironmentVariablePassword_AdminIsSkipped_AndWarns()
    {
        // No AdminAccount__Password in the environment and none in source config
        // (User Secrets are never loaded here) → seeding must skip and warn, not invent a password.
        SetPasswordEnv(null);
        var logs = new CapturingLoggerProvider();

        using var sp = BuildProvider(logs);
        await RunStartupAsync(sp);

        using var scope = sp.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.Null(await users.FindByEmailAsync(Email)); // no admin created

        Assert.Contains(logs.Warnings, w =>
            w.Contains("Password", StringComparison.OrdinalIgnoreCase) &&
            w.Contains("skipping", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void IdentitySecurity_LockoutRolesAndHashing_AreConfigured()
    {
        SetPasswordEnv(Password);
        using var sp = BuildProvider();
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
        // Never leak the process-wide env var to other tests.
        Environment.SetEnvironmentVariable(EnvVarName, null);
        SqliteConnection.ClearAllPools();
        try { Directory.Delete(_dir, recursive: true); } catch { /* temp cleanup best-effort */ }
        GC.SuppressFinalize(this);
    }

    /// <summary>Minimal in-memory logger provider that records warning-level messages.</summary>
    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        public List<string> Warnings { get; } = new();

        public ILogger CreateLogger(string categoryName) => new Capturing(this);

        public void Dispose() { }

        private sealed class Capturing : ILogger
        {
            private readonly CapturingLoggerProvider _owner;
            public Capturing(CapturingLoggerProvider owner) => _owner = owner;

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
                Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (logLevel == LogLevel.Warning)
                {
                    _owner.Warnings.Add(formatter(state, exception));
                }
            }
        }
    }
}
