using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities;

namespace ProfAly.CMS.Infrastructure.Persistence.Configurations;

// ---- Media ----
public sealed class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
{
    public void Configure(EntityTypeBuilder<MediaFile> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.StoredFileName).IsRequired().HasMaxLength(FieldLengths.StoredFileName);
        b.Property(x => x.OriginalFileName).IsRequired().HasMaxLength(FieldLengths.OriginalFileName);
        b.Property(x => x.RelativePath).IsRequired().HasMaxLength(FieldLengths.RelativePath);
        b.Property(x => x.ContentType).IsRequired().HasMaxLength(FieldLengths.MimeType);
        b.Property(x => x.AltText).HasMaxLength(FieldLengths.AltText);

        b.HasIndex(x => x.StoredFileName).IsUnique();
        b.HasIndex(x => new { x.MediaKind, x.CreatedUtc });
    }
}

// ---- Communication ----
public sealed class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(FieldLengths.ContactName);
        b.Property(x => x.Email).IsRequired().HasMaxLength(FieldLengths.Email);
        b.Property(x => x.Subject).HasMaxLength(FieldLengths.ContactSubject);
        b.Property(x => x.Message).IsRequired().HasMaxLength(FieldLengths.ContactMessage);
        b.Property(x => x.IpAddress).HasMaxLength(FieldLengths.IpAddress);
        b.Property(x => x.IsRead).HasDefaultValue(false);

        b.HasIndex(x => new { x.IsRead, x.CreatedUtc });
    }
}

// ---- SEO: Redirect ----
public sealed class RedirectConfiguration : IEntityTypeConfiguration<Redirect>
{
    public void Configure(EntityTypeBuilder<Redirect> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.FromPath).IsRequired().HasMaxLength(FieldLengths.RedirectPath);
        b.Property(x => x.ToPath).IsRequired().HasMaxLength(FieldLengths.RedirectPath);
        b.Property(x => x.Notes).HasMaxLength(FieldLengths.RedirectNotes);
        // StatusCode (301) and IsActive (true) defaults come from the domain initializer;
        // no DB-generated default so an explicit IsActive=false can always be stored.

        b.HasIndex(x => x.FromPath).IsUnique();
        b.ToTable(t => t.HasCheckConstraint("CK_Redirect_StatusCode", "StatusCode IN (301, 302)"));
    }
}

// ---- SEO: PageSeo ----
public sealed class PageSeoConfiguration : IEntityTypeConfiguration<PageSeo>
{
    public void Configure(EntityTypeBuilder<PageSeo> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.PageKey).IsRequired().HasMaxLength(FieldLengths.PageKey);
        b.Property(x => x.MetaTitle).HasMaxLength(FieldLengths.MetaTitle);
        b.Property(x => x.MetaDescription).HasMaxLength(FieldLengths.MetaDescription);
        b.Property(x => x.MetaKeywords).HasMaxLength(FieldLengths.MetaKeywords);

        b.HasIndex(x => new { x.PageKey, x.Culture }).IsUnique();
    }
}

// ---- Statistics ----
public sealed class ContentEventConfiguration : IEntityTypeConfiguration<ContentEvent>
{
    public void Configure(EntityTypeBuilder<ContentEvent> b)
    {
        b.HasKey(x => x.Id);
        // ContentItem relationship + cascade is configured on the ContentItem side.
        b.HasIndex(x => new { x.ContentItemId, x.EventType, x.CreatedUtc });
    }
}

public sealed class PageViewConfiguration : IEntityTypeConfiguration<PageView>
{
    public void Configure(EntityTypeBuilder<PageView> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Path).IsRequired().HasMaxLength(FieldLengths.EventPath);
        b.HasIndex(x => x.CreatedUtc);
    }
}
