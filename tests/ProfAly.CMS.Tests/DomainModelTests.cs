using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Domain.Entities.Content;
using ProfAly.CMS.Domain.Enums;

namespace ProfAly.CMS.Tests;

/// <summary>Stage 1 domain-model invariants (the rules that live in the domain layer).</summary>
public class DomainModelTests
{
    [Fact]
    public void ContentSubtype_ExposesCorrectDiscriminator()
    {
        Assert.Equal(ContentType.Book, new Book().ContentType);
        Assert.Equal(ContentType.Publication, new Publication().ContentType);
        Assert.Equal(ContentType.ResearchPaper, new ResearchPaper().ContentType);
        Assert.Equal(ContentType.Thesis, new Thesis().ContentType);
        Assert.Equal(ContentType.Project, new Project().ContentType);
        Assert.Equal(ContentType.Resource, new Resource().ContentType);
        Assert.Equal(ContentType.EnrichmentItem, new EnrichmentItem().ContentType);
        Assert.Equal(ContentType.Video, new Video().ContentType);
        Assert.Equal(ContentType.RecommendedBook, new RecommendedBook().ContentType);
        Assert.Equal(ContentType.Event, new Event().ContentType);
    }

    [Fact]
    public void ContentItem_GalleryImages_DefaultEmpty()
    {
        // Events (and any content item) expose an ordered gallery collection (doc 76).
        Assert.Empty(new Event().Images);
        Assert.Empty(new RecommendedBook().Images);
    }

    [Fact]
    public void RecommendedBook_AllowsPdfAndPurchaseUrl()
    {
        // RecommendedBook reuses the shared base (no extra invariants); a fully
        // populated instance is valid.
        var book = new RecommendedBook { PurchaseUrl = "https://example.com/buy", PdfFileId = 3, PublicationYear = 2021 };
        Assert.Empty(book.Validate());
    }

    [Fact]
    public void Video_RequiresValidYouTubeId_AndNoPdf()
    {
        var invalid = new Video { YouTubeVideoId = "short", PdfFileId = 5 };
        Assert.NotEmpty(invalid.Validate());

        var valid = new Video { YouTubeVideoId = "dQw4w9WgXcQ" };
        Assert.Empty(valid.Validate());
    }

    [Fact]
    public void Publication_RejectsMalformedDoi()
    {
        Assert.NotEmpty(new Publication { Doi = "not-a-doi" }.Validate());
        Assert.Empty(new Publication { Doi = "10.1000/xyz123" }.Validate());
        Assert.Empty(new Publication { Doi = null }.Validate());
    }

    [Fact]
    public void ContentItem_RejectsOutOfRangePublicationYear()
    {
        Assert.NotEmpty(new Book { PublicationYear = 1800 }.Validate());
        Assert.Empty(new Book { PublicationYear = 2020 }.Validate());
    }

    [Fact]
    public void ExperienceEntry_RejectsEndBeforeStart()
    {
        var entry = new ExperienceEntry
        {
            StartDateUtc = new DateTime(2020, 1, 1),
            EndDateUtc = new DateTime(2019, 1, 1),
        };
        Assert.NotEmpty(entry.Validate());
    }

    [Fact]
    public void Redirect_RejectsBadStatusAndSelfReference()
    {
        Assert.NotEmpty(new Redirect { FromPath = "/a", ToPath = "/a", StatusCode = 301 }.Validate());
        Assert.NotEmpty(new Redirect { FromPath = "/a", ToPath = "/b", StatusCode = 200 }.Validate());
        Assert.Empty(new Redirect { FromPath = "/about.html", ToPath = "/ar/about", StatusCode = 301 }.Validate());
    }

    [Fact]
    public void Translation_DefaultsToArabicCulture()
    {
        var translation = new ContentItemTranslation();
        Assert.IsAssignableFrom<ITranslation>(translation);
        Assert.Equal(SupportedCultures.Default, translation.Culture);
        Assert.Equal("ar", SupportedCultures.Default);
    }
}
