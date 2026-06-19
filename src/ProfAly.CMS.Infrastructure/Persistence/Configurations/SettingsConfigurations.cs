using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities;

namespace ProfAly.CMS.Infrastructure.Persistence.Configurations;

public sealed class SiteSettingsConfiguration : IEntityTypeConfiguration<SiteSettings>
{
    public void Configure(EntityTypeBuilder<SiteSettings> b)
    {
        b.HasKey(x => x.Id);

        b.Property(x => x.DefaultCulture).IsRequired().HasMaxLength(FieldLengths.Culture);
        // DefaultTheme/DefaultCulture defaults come from the domain initializer + seed
        // (no DB-generated default → avoids enum sentinel ambiguity).
        b.Property(x => x.ContactEmail).IsRequired().HasMaxLength(FieldLengths.Email);
        b.Property(x => x.FacebookUrl).HasMaxLength(FieldLengths.Url);
        b.Property(x => x.WhatsAppNumber).HasMaxLength(FieldLengths.WhatsAppNumber);
        b.Property(x => x.PrimaryColor).HasMaxLength(FieldLengths.HexColor);
        b.Property(x => x.SecondaryColor).HasMaxLength(FieldLengths.HexColor);

        b.HasOne(x => x.Logo)
            .WithMany()
            .HasForeignKey(x => x.LogoMediaId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasMany(x => x.Translations)
            .WithOne(t => t.SiteSettings!)
            .HasForeignKey(t => t.SiteSettingsId)
            .OnDelete(DeleteBehavior.Cascade);

        b.ToTable(t => t.HasCheckConstraint("CK_SiteSettings_DefaultCulture", "DefaultCulture IN ('ar','en','fr')"));
    }
}

public sealed class SiteSettingsTranslationConfiguration : IEntityTypeConfiguration<SiteSettingsTranslation>
{
    public void Configure(EntityTypeBuilder<SiteSettingsTranslation> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.SiteTitle).IsRequired().HasMaxLength(FieldLengths.SiteTitle);
        b.Property(x => x.FooterText).HasMaxLength(FieldLengths.FooterText);
        b.Property(x => x.Tagline).HasMaxLength(FieldLengths.Tagline);

        b.HasIndex(x => new { x.SiteSettingsId, x.Culture }).IsUnique();
    }
}
