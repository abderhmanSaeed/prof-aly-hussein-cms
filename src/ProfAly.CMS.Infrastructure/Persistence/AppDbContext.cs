using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Domain.Entities.Content;
using ProfAly.CMS.Domain.Enums;
using ProfAly.CMS.Infrastructure.Identity;

namespace ProfAly.CMS.Infrastructure.Persistence;

/// <summary>
/// EF Core context for the platform. Carries the ASP.NET Core Identity schema plus
/// the full v2.0 domain model. Entity mappings live in
/// <c>Persistence/Configurations</c> and are applied via assembly scan.
/// DbSet property names are chosen to match the architecture's (singular) table names.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Settings & profile
    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();
    public DbSet<SiteSettingsTranslation> SiteSettingsTranslation => Set<SiteSettingsTranslation>();
    public DbSet<Profile> Profile => Set<Profile>();
    public DbSet<ProfileTranslation> ProfileTranslation => Set<ProfileTranslation>();

    // Content (TPH)
    public DbSet<ContentItem> ContentItem => Set<ContentItem>();
    public DbSet<ContentItemTranslation> ContentItemTranslation => Set<ContentItemTranslation>();
    public DbSet<ContentItemCategory> ContentItemCategory => Set<ContentItemCategory>();

    // Taxonomy
    public DbSet<Category> Category => Set<Category>();
    public DbSet<CategoryTranslation> CategoryTranslation => Set<CategoryTranslation>();

    // Profile-page / list entities
    public DbSet<Qualification> Qualification => Set<Qualification>();
    public DbSet<QualificationTranslation> QualificationTranslation => Set<QualificationTranslation>();
    public DbSet<Skill> Skill => Set<Skill>();
    public DbSet<SkillTranslation> SkillTranslation => Set<SkillTranslation>();
    public DbSet<Membership> Membership => Set<Membership>();
    public DbSet<MembershipTranslation> MembershipTranslation => Set<MembershipTranslation>();
    public DbSet<StatItem> StatItem => Set<StatItem>();
    public DbSet<StatItemTranslation> StatItemTranslation => Set<StatItemTranslation>();
    public DbSet<Credibility> Credibility => Set<Credibility>();
    public DbSet<CredibilityTranslation> CredibilityTranslation => Set<CredibilityTranslation>();
    public DbSet<ActivityGroup> ActivityGroup => Set<ActivityGroup>();
    public DbSet<ActivityGroupTranslation> ActivityGroupTranslation => Set<ActivityGroupTranslation>();
    public DbSet<Activity> Activity => Set<Activity>();
    public DbSet<ActivityTranslation> ActivityTranslation => Set<ActivityTranslation>();

    // Page content
    public DbSet<PageSection> PageSection => Set<PageSection>();
    public DbSet<PageSectionTranslation> PageSectionTranslation => Set<PageSectionTranslation>();
    public DbSet<ExperienceEntry> ExperienceEntry => Set<ExperienceEntry>();
    public DbSet<ExperienceEntryTranslation> ExperienceEntryTranslation => Set<ExperienceEntryTranslation>();
    public DbSet<Course> Course => Set<Course>();
    public DbSet<CourseTranslation> CourseTranslation => Set<CourseTranslation>();

    // Media, communication, SEO, statistics
    public DbSet<MediaFile> MediaFile => Set<MediaFile>();
    public DbSet<ContactMessage> ContactMessage => Set<ContactMessage>();
    public DbSet<Redirect> Redirect => Set<Redirect>();
    public DbSet<PageSeo> PageSeo => Set<PageSeo>();
    public DbSet<ContentEvent> ContentEvent => Set<ContentEvent>();
    public DbSet<PageView> PageView => Set<PageView>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // Enums are stored as readable TEXT (doc 03 §1: textual discriminators/columns).
        configurationBuilder.Properties<RelationshipType>().HaveConversion<string>().HaveMaxLength(12);
        configurationBuilder.Properties<DegreeLevel>().HaveConversion<string>().HaveMaxLength(10);
        configurationBuilder.Properties<CourseLevel>().HaveConversion<string>().HaveMaxLength(15);
        configurationBuilder.Properties<MembershipKind>().HaveConversion<string>().HaveMaxLength(10);
        configurationBuilder.Properties<ProjectStatus>().HaveConversion<string>().HaveMaxLength(12);
        configurationBuilder.Properties<MediaKind>().HaveConversion<string>().HaveMaxLength(10);
        configurationBuilder.Properties<ThemeMode>().HaveConversion<string>().HaveMaxLength(10);
        configurationBuilder.Properties<ContentEventType>().HaveConversion<string>().HaveMaxLength(10);

        // The content discriminator is stored as readable TEXT (doc 03 §1.1).
        configurationBuilder.Properties<ContentType>().HaveConversion<string>().HaveMaxLength(20);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Establish the Table-Per-Hierarchy discriminator FIRST so the derived content
        // types are part of the hierarchy before their per-type configurations apply
        // (otherwise the assembly scan can register a subtype as a separate root table).
        builder.Entity<ContentItem>()
            .HasDiscriminator(e => e.ContentType)
            .HasValue<Book>(ContentType.Book)
            .HasValue<Publication>(ContentType.Publication)
            .HasValue<ResearchPaper>(ContentType.ResearchPaper)
            .HasValue<Thesis>(ContentType.Thesis)
            .HasValue<Project>(ContentType.Project)
            .HasValue<Resource>(ContentType.Resource)
            .HasValue<EnrichmentItem>(ContentType.EnrichmentItem)
            .HasValue<Video>(ContentType.Video);

        // Apply all IEntityTypeConfiguration<> in this assembly.
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Cross-cutting: every table with a Culture column is constrained to ar/en/fr
        // (doc 03 §4). Applied after configurations so table names are resolved.
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (entityType.FindProperty("Culture") is null)
            {
                continue;
            }

            var table = entityType.GetTableName();
            var entity = builder.Entity(entityType.ClrType);
            entity.Property("Culture").IsRequired().HasMaxLength(8);
            entity.ToTable(t => t.HasCheckConstraint($"CK_{table}_Culture", "Culture IN ('ar','en','fr')"));
        }
    }
}
