using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities;

namespace ProfAly.CMS.Infrastructure.Persistence.Configurations;

// ---- PageSection ----
public sealed class PageSectionConfiguration : IEntityTypeConfiguration<PageSection>
{
    public void Configure(EntityTypeBuilder<PageSection> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.PageKey).IsRequired().HasMaxLength(FieldLengths.PageKey);
        b.Property(x => x.SortOrder).HasDefaultValue(0);
        b.HasMany(x => x.Translations).WithOne(t => t.PageSection!)
            .HasForeignKey(t => t.PageSectionId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.PageKey).IsUnique();
    }
}

public sealed class PageSectionTranslationConfiguration : IEntityTypeConfiguration<PageSectionTranslation>
{
    public void Configure(EntityTypeBuilder<PageSectionTranslation> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Heading).HasMaxLength(FieldLengths.SectionHeading);
        b.HasIndex(x => new { x.PageSectionId, x.Culture }).IsUnique();
    }
}

// ---- ExperienceEntry ----
public sealed class ExperienceEntryConfiguration : IEntityTypeConfiguration<ExperienceEntry>
{
    public void Configure(EntityTypeBuilder<ExperienceEntry> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.SortOrder).HasDefaultValue(0);
        b.HasMany(x => x.Translations).WithOne(t => t.ExperienceEntry!)
            .HasForeignKey(t => t.ExperienceEntryId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.SortOrder);
    }
}

public sealed class ExperienceEntryTranslationConfiguration : IEntityTypeConfiguration<ExperienceEntryTranslation>
{
    public void Configure(EntityTypeBuilder<ExperienceEntryTranslation> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Role).IsRequired().HasMaxLength(FieldLengths.ExperienceRole);
        b.Property(x => x.Organization).IsRequired().HasMaxLength(FieldLengths.ExperienceOrganization);
        b.Property(x => x.PeriodLabel).HasMaxLength(FieldLengths.PeriodLabel);
        b.HasIndex(x => new { x.ExperienceEntryId, x.Culture }).IsUnique();
    }
}

// ---- Course ----
public sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Period).HasMaxLength(FieldLengths.CoursePeriod);
        b.Property(x => x.SortOrder).HasDefaultValue(0);
        b.HasMany(x => x.Translations).WithOne(t => t.Course!)
            .HasForeignKey(t => t.CourseId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.Level, x.SortOrder });
    }
}

public sealed class CourseTranslationConfiguration : IEntityTypeConfiguration<CourseTranslation>
{
    public void Configure(EntityTypeBuilder<CourseTranslation> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.CourseName).IsRequired().HasMaxLength(FieldLengths.CourseName);
        b.Property(x => x.Institution).HasMaxLength(FieldLengths.CourseInstitution);
        b.HasIndex(x => new { x.CourseId, x.Culture }).IsUnique();
    }
}
