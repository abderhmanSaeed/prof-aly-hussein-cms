using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Domain.Enums;

namespace ProfAly.CMS.Infrastructure.Persistence.Seeding.Seeders;

/// <summary>
/// Seeds the singleton <see cref="SiteSettings"/> with default application settings,
/// language, and theme (doc 03 §2.1). Default culture comes from
/// <c>Localization:DefaultCulture</c>; theme/contact from <c>SiteSettings:*</c>.
/// Trilingual chrome rows (ar/en/fr) are created. Idempotent.
/// </summary>
public sealed class SiteSettingsSeeder : IDataSeeder
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SiteSettingsSeeder> _logger;

    public SiteSettingsSeeder(AppDbContext context, IConfiguration configuration, ILogger<SiteSettingsSeeder> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public int Order => 3;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _context.SiteSettings.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Site settings already present; skipping.");
            return;
        }

        var defaultCulture = _configuration["Localization:DefaultCulture"];
        if (!SupportedCultures.IsSupported(defaultCulture))
        {
            defaultCulture = SupportedCultures.Default;
        }

        var defaultTheme = Enum.TryParse<ThemeMode>(_configuration["SiteSettings:DefaultTheme"], ignoreCase: true, out var theme)
            ? theme
            : ThemeMode.Light;

        var contactEmail = _configuration["SiteSettings:ContactEmail"]
            ?? _configuration["AdminAccount:Email"]
            ?? "info@example.com";

        var now = DateTime.UtcNow;
        var settings = new SiteSettings
        {
            DefaultCulture = defaultCulture!,
            DefaultTheme = defaultTheme,
            ContactEmail = contactEmail,
            CreatedUtc = now,
            ModifiedUtc = now,
            Translations = SupportedCultures.All
                .Select(culture => new SiteSettingsTranslation
                {
                    Culture = culture,
                    SiteTitle = DefaultSiteTitle(culture),
                    Tagline = DefaultTagline(culture),
                })
                .ToList(),
        };

        _context.SiteSettings.Add(settings);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Seeded site settings (culture={Culture}, theme={Theme}).", defaultCulture, defaultTheme);
    }

    private static string DefaultSiteTitle(string culture) => culture switch
    {
        SupportedCultures.Arabic => "أ. د. علي حسين",
        SupportedCultures.French => "Pr. Aly Hussein",
        _ => "Prof. Aly Hussein",
    };

    private static string DefaultTagline(string culture) => culture switch
    {
        SupportedCultures.Arabic => "أستاذ المناهج وطرق التدريس",
        SupportedCultures.French => "Professeur de curriculum et de méthodes d'enseignement",
        _ => "Professor of Curriculum & Teaching Methods",
    };
}
