using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities;

namespace ProfAly.CMS.Infrastructure.Persistence.Configurations;

public sealed class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Email).HasMaxLength(FieldLengths.Email);
        b.Property(x => x.Phone).HasMaxLength(FieldLengths.Phone);

        b.HasOne(x => x.Photo)
            .WithMany()
            .HasForeignKey(x => x.PhotoMediaId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.ContactPhoto)
            .WithMany()
            .HasForeignKey(x => x.ContactPhotoMediaId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.BioImage)
            .WithMany()
            .HasForeignKey(x => x.BioImageMediaId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.AboutImage)
            .WithMany()
            .HasForeignKey(x => x.AboutImageMediaId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasMany(x => x.Translations)
            .WithOne(t => t.Profile!)
            .HasForeignKey(t => t.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ProfileTranslationConfiguration : IEntityTypeConfiguration<ProfileTranslation>
{
    public void Configure(EntityTypeBuilder<ProfileTranslation> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.FullName).IsRequired().HasMaxLength(FieldLengths.FullName);
        b.Property(x => x.ShortName).HasMaxLength(FieldLengths.ShortName);
        b.Property(x => x.Title).IsRequired().HasMaxLength(FieldLengths.ProfileTitle);
        b.Property(x => x.Positioning).HasMaxLength(FieldLengths.Positioning);
        b.Property(x => x.ShortBio).HasMaxLength(FieldLengths.ShortBio);
        b.Property(x => x.Nationality).HasMaxLength(FieldLengths.Nationality);
        b.Property(x => x.MaritalStatus).HasMaxLength(FieldLengths.MaritalStatus);
        b.Property(x => x.Location).HasMaxLength(FieldLengths.Location);
        b.Property(x => x.Languages).HasMaxLength(FieldLengths.Languages);

        b.HasOne(x => x.CvFile)
            .WithMany()
            .HasForeignKey(x => x.CvFileId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasIndex(x => new { x.ProfileId, x.Culture }).IsUnique();
    }
}
