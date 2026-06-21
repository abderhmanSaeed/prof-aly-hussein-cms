namespace ProfAly.CMS.Domain.Common;

/// <summary>
/// Central maximum string lengths from the entity catalogue (doc 05). Shared so
/// EF configuration (Stage 2), data-annotation/FluentValidation rules, and admin
/// forms stay in agreement. These are persistence/presentation limits, not domain
/// invariants — the domain enforces only the cross-field rules in <see cref="IValidatableEntity"/>.
/// </summary>
public static class FieldLengths
{
    public const int Culture = 2;

    // Shared content / translation text
    public const int Title = 250;
    public const int Slug = 250;
    public const int Summary = 600;
    public const int MetaTitle = 70;
    public const int MetaDescription = 160;
    public const int MetaKeywords = 250;

    // Content type-specific (translatable)
    public const int Journal = 250;
    public const int Authors = 500;
    public const int Publisher = 250;
    public const int AuthorshipRole = 150;
    public const int ResearcherName = 200;
    public const int EventLocation = 250;

    // Content type-specific (non-translatable)
    public const int Doi = 100;
    public const int ExternalUrl = 500;
    public const int YouTubeVideoId = 20;
    public const int ProjectRole = 150;
    public const int ResourceType = 50;
    public const int PurchaseUrl = 500;
    public const int ImageCaption = 250;

    // Profile
    public const int FullName = 150;
    public const int ShortName = 80;
    public const int ProfileTitle = 200;
    public const int Positioning = 300;
    public const int ShortBio = 500;
    public const int Nationality = 100;
    public const int MaritalStatus = 100;
    public const int Location = 150;
    public const int Languages = 250;

    // Site settings
    public const int SiteTitle = 150;
    public const int FooterText = 500;
    public const int Tagline = 200;
    public const int Url = 300;
    public const int WhatsAppNumber = 30;
    public const int HexColor = 9;

    // Contact
    public const int Email = 254;
    public const int Phone = 30;
    public const int ContactName = 150;
    public const int ContactSubject = 200;
    public const int ContactMessage = 4000;
    public const int IpAddress = 45;

    // List entities
    public const int CategoryName = 120;
    public const int CategorySlug = 120;
    public const int QualificationDegree = 200;
    public const int QualificationInstitution = 200;
    public const int QualificationGrade = 150;
    public const int SkillName = 200;
    public const int MembershipName = 250;
    public const int StatLabel = 150;
    public const int StatSuffix = 5;
    public const int CredibilityName = 150;
    public const int ActivityGroupName = 200;
    public const int ActivityText = 1000;
    public const int SectionHeading = 200;
    public const int ExperienceRole = 200;
    public const int ExperienceOrganization = 200;
    public const int PeriodLabel = 120;
    public const int CourseName = 200;
    public const int CourseInstitution = 200;
    public const int CoursePeriod = 50;

    // Media
    public const int StoredFileName = 120;
    public const int OriginalFileName = 260;
    public const int RelativePath = 300;
    public const int MimeType = 120;
    public const int AltText = 250;

    // SEO / routing
    public const int PageKey = 50;
    public const int RedirectPath = 400;
    public const int RedirectNotes = 250;
    public const int EventPath = 300;
}
