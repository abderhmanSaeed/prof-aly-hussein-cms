using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Domain.Entities.Content;

namespace ProfAly.CMS.Infrastructure.Persistence.Configurations;

/// <summary>TPH base + discriminator + shared mappings for all content types.</summary>
public sealed class ContentItemConfiguration : IEntityTypeConfiguration<ContentItem>
{
    public void Configure(EntityTypeBuilder<ContentItem> b)
    {
        b.HasKey(x => x.Id);

        // The TPH discriminator (column "ContentType") is established in
        // AppDbContext.OnModelCreating before configurations are applied, so the
        // derived types are part of the hierarchy when their per-type configs run.
        b.Property(x => x.ExternalUrl).HasMaxLength(FieldLengths.ExternalUrl);
        b.Property(x => x.IsPublished).HasDefaultValue(false);
        b.Property(x => x.IsFeatured).HasDefaultValue(false);
        b.Property(x => x.SortOrder).HasDefaultValue(0);
        b.Property(x => x.ViewCount).HasDefaultValue(0);
        b.Property(x => x.DownloadCount).HasDefaultValue(0);

        b.HasOne(x => x.CoverImage)
            .WithMany()
            .HasForeignKey(x => x.CoverImageId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.PdfFile)
            .WithMany()
            .HasForeignKey(x => x.PdfFileId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasMany(x => x.Translations)
            .WithOne(t => t.ContentItem!)
            .HasForeignKey(t => t.ContentItemId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(x => x.Events)
            .WithOne(e => e.ContentItem!)
            .HasForeignKey(e => e.ContentItemId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(x => x.Images)
            .WithOne(i => i.ContentItem!)
            .HasForeignKey(i => i.ContentItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Fast published listings per section + featured selection.
        b.HasIndex("ContentType", nameof(ContentItem.IsPublished), nameof(ContentItem.SortOrder));
        b.HasIndex("ContentType", nameof(ContentItem.IsFeatured), nameof(ContentItem.IsPublished));
    }
}

public sealed class PublicationConfiguration : IEntityTypeConfiguration<Publication>
{
    public void Configure(EntityTypeBuilder<Publication> b) =>
        b.Property(x => x.Doi).HasMaxLength(FieldLengths.Doi);
}

public sealed class ResearchPaperConfiguration : IEntityTypeConfiguration<ResearchPaper>
{
    public void Configure(EntityTypeBuilder<ResearchPaper> b) =>
        b.Property(x => x.Doi).HasMaxLength(FieldLengths.Doi);
}

public sealed class ThesisConfiguration : IEntityTypeConfiguration<Thesis>
{
    public void Configure(EntityTypeBuilder<Thesis> b)
    {
        // Public theses-table facets (RelationshipType + DegreeLevel are Thesis-only columns).
        b.HasIndex(x => new { x.RelationshipType, x.DegreeLevel });
    }
}

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> b) =>
        b.Property(x => x.Role).HasMaxLength(FieldLengths.ProjectRole);
}

public sealed class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> b) =>
        b.Property(x => x.ResourceType).HasMaxLength(FieldLengths.ResourceType);
}

public sealed class EnrichmentItemConfiguration : IEntityTypeConfiguration<EnrichmentItem>
{
    public void Configure(EntityTypeBuilder<EnrichmentItem> b) =>
        b.Property(x => x.ResourceType).HasMaxLength(FieldLengths.ResourceType);
}

public sealed class VideoConfiguration : IEntityTypeConfiguration<Video>
{
    public void Configure(EntityTypeBuilder<Video> b) =>
        b.Property(x => x.YouTubeVideoId).HasMaxLength(FieldLengths.YouTubeVideoId);
}

public sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    // Optional event video id lives in its own nullable TPH column so the existing Video
    // column is left completely untouched. Same format/length as Video's id (doc 90).
    public void Configure(EntityTypeBuilder<Event> b) =>
        b.Property(x => x.VideoYouTubeId)
            .HasColumnName("EventVideoYouTubeId")
            .HasMaxLength(FieldLengths.YouTubeVideoId);
}

public sealed class RecommendedBookConfiguration : IEntityTypeConfiguration<RecommendedBook>
{
    public void Configure(EntityTypeBuilder<RecommendedBook> b) =>
        b.Property(x => x.PurchaseUrl).HasMaxLength(FieldLengths.PurchaseUrl);
}

public sealed class ContentImageConfiguration : IEntityTypeConfiguration<ContentImage>
{
    public void Configure(EntityTypeBuilder<ContentImage> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Caption).HasMaxLength(FieldLengths.ImageCaption);

        // Gallery image points at a MediaFile; restrict so deleting an in-use media row
        // is blocked (the gallery link must be removed first).
        b.HasOne(x => x.MediaFile)
            .WithMany()
            .HasForeignKey(x => x.MediaFileId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.ContentItemId, x.SortOrder });
    }
}

public sealed class ContentItemTranslationConfiguration : IEntityTypeConfiguration<ContentItemTranslation>
{
    public void Configure(EntityTypeBuilder<ContentItemTranslation> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).IsRequired().HasMaxLength(FieldLengths.Title);
        b.Property(x => x.Slug).IsRequired().HasMaxLength(FieldLengths.Slug);
        b.Property(x => x.Summary).HasMaxLength(FieldLengths.Summary);
        b.Property(x => x.Journal).HasMaxLength(FieldLengths.Journal);
        b.Property(x => x.Authors).HasMaxLength(FieldLengths.Authors);
        b.Property(x => x.Publisher).HasMaxLength(FieldLengths.Publisher);
        b.Property(x => x.AuthorshipRole).HasMaxLength(FieldLengths.AuthorshipRole);
        b.Property(x => x.ResearcherName).HasMaxLength(FieldLengths.ResearcherName);
        b.Property(x => x.Location).HasMaxLength(FieldLengths.EventLocation);
        b.Property(x => x.MetaTitle).HasMaxLength(FieldLengths.MetaTitle);
        b.Property(x => x.MetaDescription).HasMaxLength(FieldLengths.MetaDescription);
        b.Property(x => x.MetaKeywords).HasMaxLength(FieldLengths.MetaKeywords);

        b.HasIndex(x => new { x.ContentItemId, x.Culture }).IsUnique();

        // Slug uniqueness is enforced per culture at the database level (routing-safe).
        // Per-(ContentType, Culture) relaxation, if ever needed, is enforced in the
        // application layer — a single index cannot span the parent's discriminator.
        b.HasIndex(x => new { x.Culture, x.Slug }).IsUnique();
    }
}

public sealed class ContentItemCategoryConfiguration : IEntityTypeConfiguration<ContentItemCategory>
{
    public void Configure(EntityTypeBuilder<ContentItemCategory> b)
    {
        b.HasKey(x => new { x.ContentItemId, x.CategoryId });

        b.HasOne(x => x.ContentItem)
            .WithMany(c => c.Categories)
            .HasForeignKey(x => x.ContentItemId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Category)
            .WithMany(c => c.ContentLinks)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.CategoryId);
    }
}
