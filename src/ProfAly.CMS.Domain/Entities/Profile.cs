using ProfAly.CMS.Domain.Common;

namespace ProfAly.CMS.Domain.Entities;

/// <summary>
/// Singleton (Id = 1) for the professor's identity (doc 03 §2.1 / doc 05 §2).
/// Culture-neutral data lives here; localised text lives on <see cref="ProfileTranslation"/>.
/// </summary>
public class Profile : AuditableEntity, IValidatableEntity
{
    public int? PhotoMediaId { get; set; }

    public MediaFile? Photo { get; set; }

    /// <summary>Dedicated portrait for the public Contact page (doc 05 §2). Falls back to <see cref="Photo"/> when unset.</summary>
    public int? ContactPhotoMediaId { get; set; }

    public MediaFile? ContactPhoto { get; set; }

    /// <summary>Dedicated image for the homepage biography / "About" snapshot section. Independent of <see cref="Photo"/>.</summary>
    public int? BioImageMediaId { get; set; }

    public MediaFile? BioImage { get; set; }

    /// <summary>Dedicated image for the public About page biography section. Independent of <see cref="Photo"/> / <see cref="BioImage"/>.</summary>
    public int? AboutImageMediaId { get; set; }

    public MediaFile? AboutImage { get; set; }

    /// <summary>Stored as a date; formatted per culture at render (doc 10 §8b).</summary>
    public DateTime? DateOfBirth { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public ICollection<ProfileTranslation> Translations { get; set; } = new List<ProfileTranslation>();

    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (DateOfBirth.HasValue && DateOfBirth.Value.Date > DateTime.UtcNow.Date)
        {
            errors.Add("Date of birth cannot be in the future.");
        }

        return errors;
    }
}

/// <summary>Per-culture profile text, personal details, and CV download (doc 03 §2.1).</summary>
public class ProfileTranslation : BaseEntity, ITranslation
{
    public int ProfileId { get; set; }

    public Profile? Profile { get; set; }

    public string Culture { get; set; } = SupportedCultures.Default;

    public string FullName { get; set; } = string.Empty;

    public string? ShortName { get; set; }

    public string Title { get; set; } = string.Empty;

    /// <summary>Hero one-line positioning statement.</summary>
    public string? Positioning { get; set; }

    public string? ShortBio { get; set; }

    /// <summary>Multi-paragraph rich-text biography.</summary>
    public string? FullBio { get; set; }

    public string? Nationality { get; set; }

    public string? MaritalStatus { get; set; }

    public string? Location { get; set; }

    public string? Languages { get; set; }

    /// <summary>
    /// Dedicated rich-text intro for the public Contact page's details panel.
    /// Independent of <see cref="ShortBio"/> so the Contact section can be
    /// authored and formatted separately. Optional; the panel hides it when null.
    /// </summary>
    public string? ContactIntro { get; set; }

    /// <summary>Per-culture CV PDF (doc 03 §2.1) — enables "Download CV" per language.</summary>
    public int? CvFileId { get; set; }

    public MediaFile? CvFile { get; set; }
}
