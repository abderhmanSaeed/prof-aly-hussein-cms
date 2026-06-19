using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Domain.Entities.Content;
using ProfAly.CMS.Domain.Enums;
using ProfileEntity = ProfAly.CMS.Domain.Entities.Profile;

namespace ProfAly.CMS.Infrastructure.Persistence.Seeding.Seeders;

/// <summary>
/// Imports the legacy static-site content (converted from data.js → embedded JSON)
/// into the CMS — Profile, stats, credibility, qualifications, experience, skills,
/// memberships, courses, activities, and content items (books/publications/theses).
/// Runs only when <c>Seed:ImportStaticContent = true</c>; each dataset is imported
/// only if its target table is empty (idempotent). French is left empty (fallback).
/// See 35_Content_Migration_Plan.md.
/// </summary>
public sealed class StaticContentImporter : IDataSeeder
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<StaticContentImporter> _logger;

    public StaticContentImporter(AppDbContext db, IConfiguration config, ILogger<StaticContentImporter> logger)
    {
        _db = db;
        _config = config;
        _logger = logger;
    }

    public int Order => 100;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!_config.GetValue("Seed:ImportStaticContent", false))
        {
            return;
        }

        var data = LoadData();
        if (data is null)
        {
            _logger.LogWarning("Static content JSON not found; skipping import.");
            return;
        }

        _logger.LogInformation("Importing static content…");
        await ImportProfileAsync(data, cancellationToken);
        await ImportListsAsync(data, cancellationToken);
        await ImportActivitiesAsync(data, cancellationToken);
        await ImportContentAsync(data, cancellationToken);
        _logger.LogInformation("Static content import complete.");
    }

    // ---------------- Profile ----------------
    private async Task ImportProfileAsync(StaticData d, CancellationToken ct)
    {
        if (await _db.Profile.AnyAsync(ct) || d.Profile is null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var p = new ProfileEntity
        {
            Email = d.Profile.Email,
            Phone = d.Profile.Phone,
            DateOfBirth = ParseEnglishDate(d.Profile.Born?.En),
            CreatedUtc = now,
            ModifiedUtc = now,
        };

        foreach (var c in SupportedCultures.All)
        {
            var name = Pick(d.Profile.Name, c);
            if (c != SupportedCultures.Default && string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            p.Translations.Add(new ProfileTranslation
            {
                Culture = c,
                FullName = name ?? string.Empty,
                ShortName = Pick(d.Profile.ShortName, c),
                Title = Pick(d.Profile.Title, c) ?? string.Empty,
                Positioning = Pick(d.Profile.Positioning, c),
                Nationality = Pick(d.Profile.Nationality, c),
                MaritalStatus = Pick(d.Profile.Marital, c),
                Location = Pick(d.Profile.Location, c),
                Languages = Pick(d.Profile.Languages, c),
                FullBio = JoinBio(d.Bio, c),
            });
        }

        _db.Profile.Add(p);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Imported profile.");
    }

    // ---------------- Independent lists ----------------
    private async Task ImportListsAsync(StaticData d, CancellationToken ct)
    {
        if (d.Stats is { Count: > 0 } && !await _db.StatItem.AnyAsync(ct))
        {
            for (var i = 0; i < d.Stats.Count; i++)
            {
                var s = d.Stats[i];
                var e = new StatItem { Value = s.Value, Suffix = NullIfEmpty(s.Suffix), SortOrder = i, CreatedUtc = DateTime.UtcNow, ModifiedUtc = DateTime.UtcNow };
                AddTranslations(SupportedCultures.All, s.Label, (c, v) => e.Translations.Add(new StatItemTranslation { Culture = c, Label = v }));
                _db.StatItem.Add(e);
            }
        }

        if (d.Credibility is { Count: > 0 } && !await _db.Credibility.AnyAsync(ct))
        {
            for (var i = 0; i < d.Credibility.Count; i++)
            {
                var e = new Credibility { SortOrder = i, CreatedUtc = DateTime.UtcNow, ModifiedUtc = DateTime.UtcNow };
                AddTranslations(SupportedCultures.All, d.Credibility[i], (c, v) => e.Translations.Add(new CredibilityTranslation { Culture = c, Name = v }));
                _db.Credibility.Add(e);
            }
        }

        if (d.Qualifications is { Count: > 0 } && !await _db.Qualification.AnyAsync(ct))
        {
            for (var i = 0; i < d.Qualifications.Count; i++)
            {
                var q = d.Qualifications[i];
                var e = new Qualification { Year = q.Year, SortOrder = i, CreatedUtc = DateTime.UtcNow, ModifiedUtc = DateTime.UtcNow };
                foreach (var c in SupportedCultures.All)
                {
                    var degree = Pick(q.Degree, c);
                    if (c != SupportedCultures.Default && string.IsNullOrWhiteSpace(degree)) continue;
                    e.Translations.Add(new QualificationTranslation { Culture = c, Degree = degree ?? string.Empty, Institution = Pick(q.Institution, c) ?? string.Empty, Grade = Pick(q.Grade, c) });
                }
                _db.Qualification.Add(e);
            }
        }

        if (d.Skills is { Count: > 0 } && !await _db.Skill.AnyAsync(ct))
        {
            for (var i = 0; i < d.Skills.Count; i++)
            {
                var e = new Skill { SortOrder = i, CreatedUtc = DateTime.UtcNow, ModifiedUtc = DateTime.UtcNow };
                AddTranslations(SupportedCultures.All, d.Skills[i], (c, v) => e.Translations.Add(new SkillTranslation { Culture = c, Name = v }));
                _db.Skill.Add(e);
            }
        }

        if (d.Memberships is not null && !await _db.Membership.AnyAsync(ct))
        {
            AddMemberships(d.Memberships.Societies, MembershipKind.Society);
            AddMemberships(d.Memberships.Boards, MembershipKind.Board);
        }

        if (d.Career is { Count: > 0 } && !await _db.ExperienceEntry.AnyAsync(ct))
        {
            for (var i = 0; i < d.Career.Count; i++)
            {
                var c0 = d.Career[i];
                var e = new ExperienceEntry { SortOrder = i, CreatedUtc = DateTime.UtcNow, ModifiedUtc = DateTime.UtcNow };
                foreach (var c in SupportedCultures.All)
                {
                    var role = Pick(c0.Role, c);
                    if (c != SupportedCultures.Default && string.IsNullOrWhiteSpace(role)) continue;
                    e.Translations.Add(new ExperienceEntryTranslation
                    {
                        Culture = c,
                        Role = role ?? string.Empty,
                        Organization = Pick(c0.Org, c) ?? string.Empty,
                        Description = Pick(c0.Desc, c),
                        PeriodLabel = Pick(c0.Period, c),
                    });
                }
                _db.ExperienceEntry.Add(e);
            }
        }

        if (d.Teaching is not null && !await _db.Course.AnyAsync(ct))
        {
            AddCourses(d.Teaching.Undergraduate, CourseLevel.Undergraduate);
            AddCourses(d.Teaching.Graduate, CourseLevel.Graduate);
        }

        await _db.SaveChangesAsync(ct);
    }

    private void AddMemberships(List<Loc>? items, MembershipKind kind)
    {
        if (items is null) return;
        for (var i = 0; i < items.Count; i++)
        {
            var e = new Membership { Kind = kind, SortOrder = i, CreatedUtc = DateTime.UtcNow, ModifiedUtc = DateTime.UtcNow };
            AddTranslations(SupportedCultures.All, items[i], (c, v) => e.Translations.Add(new MembershipTranslation { Culture = c, Name = v }));
            _db.Membership.Add(e);
        }
    }

    private void AddCourses(List<Loc>? items, CourseLevel level)
    {
        if (items is null) return;
        for (var i = 0; i < items.Count; i++)
        {
            var e = new Course { Level = level, SortOrder = i, CreatedUtc = DateTime.UtcNow, ModifiedUtc = DateTime.UtcNow };
            AddTranslations(SupportedCultures.All, items[i], (c, v) => e.Translations.Add(new CourseTranslation { Culture = c, CourseName = v }));
            _db.Course.Add(e);
        }
    }

    // ---------------- Activities ----------------
    private async Task ImportActivitiesAsync(StaticData d, CancellationToken ct)
    {
        if (d.Activities is not { Count: > 0 } || await _db.ActivityGroup.AnyAsync(ct))
        {
            return;
        }

        for (var gi = 0; gi < d.Activities.Count; gi++)
        {
            var g = d.Activities[gi];
            var group = new ActivityGroup { SortOrder = gi, CreatedUtc = DateTime.UtcNow, ModifiedUtc = DateTime.UtcNow };
            AddTranslations(SupportedCultures.All, g.Group, (c, v) => group.Translations.Add(new ActivityGroupTranslation { Culture = c, Name = v }));

            for (var ii = 0; g.Items is not null && ii < g.Items.Count; ii++)
            {
                var item = g.Items[ii];
                var activity = new Activity { SortOrder = ii, CreatedUtc = DateTime.UtcNow, ModifiedUtc = DateTime.UtcNow };
                AddTranslations(SupportedCultures.All, item, (c, v) => activity.Translations.Add(new ActivityTranslation { Culture = c, Text = v }));
                group.Activities.Add(activity);
            }

            _db.ActivityGroup.Add(group);
        }

        await _db.SaveChangesAsync(ct);
    }

    // ---------------- Content (books / publications / theses) ----------------
    private async Task ImportContentAsync(StaticData d, CancellationToken ct)
    {
        if (await _db.ContentItem.AnyAsync(ct))
        {
            return;
        }

        // Seed slug-uniqueness tracker from any existing translations (none expected here).
        var usedSlugs = new Dictionary<string, HashSet<string>>();
        foreach (var c in SupportedCultures.All)
        {
            usedSlugs[c] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        var order = 0;
        foreach (var b in d.Books ?? new())
        {
            var item = new Book { PublicationYear = b.Year, IsFeatured = b.Featured, IsPublished = true, SortOrder = order++, CreatedUtc = DateTime.UtcNow, ModifiedUtc = DateTime.UtcNow };
            foreach (var c in SupportedCultures.All)
            {
                var title = Pick(b.Title, c);
                if (c != SupportedCultures.Default && string.IsNullOrWhiteSpace(title)) continue;
                item.Translations.Add(new ContentItemTranslation
                {
                    Culture = c,
                    Title = title ?? string.Empty,
                    Slug = UniqueSlug(usedSlugs[c], title),
                    Publisher = Pick(b.Publisher, c),
                    AuthorshipRole = Pick(b.Role, c),
                });
            }
            _db.ContentItem.Add(item);
        }

        order = 0;
        foreach (var p in d.Publications ?? new())
        {
            var item = new Publication { PublicationYear = p.Year, IsPublished = true, SortOrder = order++, CreatedUtc = DateTime.UtcNow, ModifiedUtc = DateTime.UtcNow };
            foreach (var c in SupportedCultures.All)
            {
                var title = Pick(p.Title, c);
                if (c != SupportedCultures.Default && string.IsNullOrWhiteSpace(title)) continue;
                item.Translations.Add(new ContentItemTranslation
                {
                    Culture = c,
                    Title = title ?? string.Empty,
                    Slug = UniqueSlug(usedSlugs[c], title),
                    Journal = Pick(p.Venue, c),
                });
            }
            _db.ContentItem.Add(item);
        }

        order = 0;
        foreach (var t in d.Theses ?? new())
        {
            var item = new Thesis
            {
                PublicationYear = t.Year,
                DegreeLevel = string.Equals(t.Degree, "PhD", StringComparison.OrdinalIgnoreCase) ? DegreeLevel.PhD : DegreeLevel.Master,
                RelationshipType = t.Category switch { "examined" => RelationshipType.Examined, "ongoing" => RelationshipType.Ongoing, _ => RelationshipType.Supervised },
                IsPublished = true,
                SortOrder = order++,
                CreatedUtc = DateTime.UtcNow,
                ModifiedUtc = DateTime.UtcNow,
            };
            foreach (var c in SupportedCultures.All)
            {
                var title = Pick(t.Title, c);
                if (c != SupportedCultures.Default && string.IsNullOrWhiteSpace(title)) continue;
                item.Translations.Add(new ContentItemTranslation
                {
                    Culture = c,
                    Title = title ?? string.Empty,
                    Slug = UniqueSlug(usedSlugs[c], title),
                    ResearcherName = Pick(t.Researcher, c),
                });
            }
            _db.ContentItem.Add(item);
        }

        await _db.SaveChangesAsync(ct);
    }

    // ---------------- Helpers ----------------
    private static void AddTranslations(IEnumerable<string> cultures, Loc? loc, Action<string, string> add)
    {
        foreach (var c in cultures)
        {
            var value = Pick(loc, c);
            if (c != SupportedCultures.Default && string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            add(c, value ?? string.Empty);
        }
    }

    private static string? Pick(Loc? loc, string culture) => culture switch
    {
        SupportedCultures.Arabic => loc?.Ar,
        SupportedCultures.English => loc?.En,
        _ => null, // no French in the legacy data
    };

    private static string? JoinBio(BioData? bio, string culture)
    {
        var paras = culture switch
        {
            SupportedCultures.Arabic => bio?.Ar,
            SupportedCultures.English => bio?.En,
            _ => null,
        };
        return paras is { Count: > 0 } ? string.Join("\n\n", paras) : null;
    }

    private static string UniqueSlug(HashSet<string> used, string? title)
    {
        var baseSlug = Slugify(title);
        if (string.IsNullOrEmpty(baseSlug)) baseSlug = "item";
        var candidate = baseSlug;
        var n = 2;
        while (!used.Add(candidate))
        {
            candidate = $"{baseSlug}-{n++}";
        }
        return candidate;
    }

    private static string Slugify(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var sb = new StringBuilder();
        var dash = false;
        foreach (var ch in input.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch)) { sb.Append(ch); dash = false; }
            else if (!dash && sb.Length > 0) { sb.Append('-'); dash = true; }
        }
        return sb.ToString().Trim('-');
    }

    private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;

    private static DateTime? ParseEnglishDate(string? value) =>
        DateTime.TryParse(value, System.Globalization.CultureInfo.GetCultureInfo("en-US"),
            System.Globalization.DateTimeStyles.None, out var d) ? d : null;

    private static StaticData? LoadData()
    {
        var asm = Assembly.GetExecutingAssembly();
        var name = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("static-content.json", StringComparison.OrdinalIgnoreCase));
        if (name is null) return null;
        using var stream = asm.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        return JsonSerializer.Deserialize<StaticData>(json, JsonOptions);
    }

    // ---------------- JSON model (matches data.js) ----------------
    private sealed class Loc { public string? Ar { get; set; } public string? En { get; set; } }
    private sealed class BioData { public List<string>? Ar { get; set; } public List<string>? En { get; set; } }

    private sealed class StaticData
    {
        public ProfileData? Profile { get; set; }
        public BioData? Bio { get; set; }
        public List<StatData>? Stats { get; set; }
        public List<Loc>? Credibility { get; set; }
        public List<QualData>? Qualifications { get; set; }
        public List<CareerData>? Career { get; set; }
        public List<Loc>? Skills { get; set; }
        public List<BookData>? Books { get; set; }
        public List<PubData>? Publications { get; set; }
        public List<ThesisData>? Theses { get; set; }
        public List<ActivityData>? Activities { get; set; }
        public TeachingData? Teaching { get; set; }
        public MembershipData? Memberships { get; set; }
    }

    private sealed class ProfileData
    {
        public Loc? Name { get; set; }
        public Loc? ShortName { get; set; }
        public Loc? Title { get; set; }
        public Loc? Positioning { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public Loc? Location { get; set; }
        public Loc? Born { get; set; }
        public Loc? Nationality { get; set; }
        public Loc? Marital { get; set; }
        public Loc? Languages { get; set; }
    }

    private sealed class StatData { public int Value { get; set; } public string? Suffix { get; set; } public Loc? Label { get; set; } }
    private sealed class QualData { public int? Year { get; set; } public Loc? Grade { get; set; } public Loc? Degree { get; set; } public Loc? Institution { get; set; } }
    private sealed class CareerData { public Loc? Period { get; set; } public Loc? Role { get; set; } public Loc? Org { get; set; } public Loc? Desc { get; set; } }
    private sealed class BookData { public int? Year { get; set; } public bool Featured { get; set; } public Loc? Publisher { get; set; } public Loc? Title { get; set; } public Loc? Role { get; set; } }
    private sealed class PubData { public int? Year { get; set; } public Loc? Title { get; set; } public Loc? Venue { get; set; } }
    private sealed class ThesisData { public int N { get; set; } public string? Category { get; set; } public string? Degree { get; set; } public int? Year { get; set; } public Loc? Researcher { get; set; } public Loc? Title { get; set; } }
    private sealed class ActivityData { public Loc? Group { get; set; } public List<Loc>? Items { get; set; } }
    private sealed class TeachingData { public List<Loc>? Undergraduate { get; set; } public List<Loc>? Graduate { get; set; } }
    private sealed class MembershipData { public List<Loc>? Societies { get; set; } public List<Loc>? Boards { get; set; } }
}
