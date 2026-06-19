using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities;

namespace ProfAly.CMS.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.SortOrder).HasDefaultValue(0);

        b.HasMany(x => x.Translations)
            .WithOne(t => t.Category!)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.SortOrder);
    }
}

public sealed class CategoryTranslationConfiguration : IEntityTypeConfiguration<CategoryTranslation>
{
    public void Configure(EntityTypeBuilder<CategoryTranslation> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(FieldLengths.CategoryName);
        b.Property(x => x.Slug).IsRequired().HasMaxLength(FieldLengths.CategorySlug);

        b.HasIndex(x => new { x.CategoryId, x.Culture }).IsUnique();
        b.HasIndex(x => new { x.Culture, x.Slug }).IsUnique();
    }
}
