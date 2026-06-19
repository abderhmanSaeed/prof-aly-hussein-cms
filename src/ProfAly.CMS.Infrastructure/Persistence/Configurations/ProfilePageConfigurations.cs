using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities;

namespace ProfAly.CMS.Infrastructure.Persistence.Configurations;

// ---- Qualification ----
public sealed class QualificationConfiguration : IEntityTypeConfiguration<Qualification>
{
    public void Configure(EntityTypeBuilder<Qualification> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.SortOrder).HasDefaultValue(0);
        b.HasMany(x => x.Translations).WithOne(t => t.Qualification!)
            .HasForeignKey(t => t.QualificationId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.SortOrder);
    }
}

public sealed class QualificationTranslationConfiguration : IEntityTypeConfiguration<QualificationTranslation>
{
    public void Configure(EntityTypeBuilder<QualificationTranslation> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Degree).IsRequired().HasMaxLength(FieldLengths.QualificationDegree);
        b.Property(x => x.Institution).IsRequired().HasMaxLength(FieldLengths.QualificationInstitution);
        b.Property(x => x.Grade).HasMaxLength(FieldLengths.QualificationGrade);
        b.HasIndex(x => new { x.QualificationId, x.Culture }).IsUnique();
    }
}

// ---- Skill ----
public sealed class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.SortOrder).HasDefaultValue(0);
        b.HasMany(x => x.Translations).WithOne(t => t.Skill!)
            .HasForeignKey(t => t.SkillId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.SortOrder);
    }
}

public sealed class SkillTranslationConfiguration : IEntityTypeConfiguration<SkillTranslation>
{
    public void Configure(EntityTypeBuilder<SkillTranslation> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(FieldLengths.SkillName);
        b.HasIndex(x => new { x.SkillId, x.Culture }).IsUnique();
    }
}

// ---- Membership ----
public sealed class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.SortOrder).HasDefaultValue(0);
        b.HasMany(x => x.Translations).WithOne(t => t.Membership!)
            .HasForeignKey(t => t.MembershipId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.Kind, x.SortOrder });
    }
}

public sealed class MembershipTranslationConfiguration : IEntityTypeConfiguration<MembershipTranslation>
{
    public void Configure(EntityTypeBuilder<MembershipTranslation> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(FieldLengths.MembershipName);
        b.HasIndex(x => new { x.MembershipId, x.Culture }).IsUnique();
    }
}

// ---- StatItem ----
public sealed class StatItemConfiguration : IEntityTypeConfiguration<StatItem>
{
    public void Configure(EntityTypeBuilder<StatItem> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Suffix).HasMaxLength(FieldLengths.StatSuffix);
        b.Property(x => x.SortOrder).HasDefaultValue(0);
        b.HasMany(x => x.Translations).WithOne(t => t.StatItem!)
            .HasForeignKey(t => t.StatItemId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.SortOrder);
    }
}

public sealed class StatItemTranslationConfiguration : IEntityTypeConfiguration<StatItemTranslation>
{
    public void Configure(EntityTypeBuilder<StatItemTranslation> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Label).IsRequired().HasMaxLength(FieldLengths.StatLabel);
        b.HasIndex(x => new { x.StatItemId, x.Culture }).IsUnique();
    }
}

// ---- Credibility ----
public sealed class CredibilityConfiguration : IEntityTypeConfiguration<Credibility>
{
    public void Configure(EntityTypeBuilder<Credibility> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.SortOrder).HasDefaultValue(0);
        b.HasOne(x => x.Logo).WithMany()
            .HasForeignKey(x => x.LogoMediaId).OnDelete(DeleteBehavior.SetNull);
        b.HasMany(x => x.Translations).WithOne(t => t.Credibility!)
            .HasForeignKey(t => t.CredibilityId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.SortOrder);
    }
}

public sealed class CredibilityTranslationConfiguration : IEntityTypeConfiguration<CredibilityTranslation>
{
    public void Configure(EntityTypeBuilder<CredibilityTranslation> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(FieldLengths.CredibilityName);
        b.HasIndex(x => new { x.CredibilityId, x.Culture }).IsUnique();
    }
}

// ---- ActivityGroup + Activity ----
public sealed class ActivityGroupConfiguration : IEntityTypeConfiguration<ActivityGroup>
{
    public void Configure(EntityTypeBuilder<ActivityGroup> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.SortOrder).HasDefaultValue(0);
        b.HasMany(x => x.Translations).WithOne(t => t.ActivityGroup!)
            .HasForeignKey(t => t.ActivityGroupId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Activities).WithOne(a => a.ActivityGroup!)
            .HasForeignKey(a => a.ActivityGroupId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.SortOrder);
    }
}

public sealed class ActivityGroupTranslationConfiguration : IEntityTypeConfiguration<ActivityGroupTranslation>
{
    public void Configure(EntityTypeBuilder<ActivityGroupTranslation> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(FieldLengths.ActivityGroupName);
        b.HasIndex(x => new { x.ActivityGroupId, x.Culture }).IsUnique();
    }
}

public sealed class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.SortOrder).HasDefaultValue(0);
        b.HasMany(x => x.Translations).WithOne(t => t.Activity!)
            .HasForeignKey(t => t.ActivityId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.ActivityGroupId, x.SortOrder });
    }
}

public sealed class ActivityTranslationConfiguration : IEntityTypeConfiguration<ActivityTranslation>
{
    public void Configure(EntityTypeBuilder<ActivityTranslation> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Text).IsRequired().HasMaxLength(FieldLengths.ActivityText);
        b.HasIndex(x => new { x.ActivityId, x.Culture }).IsUnique();
    }
}
