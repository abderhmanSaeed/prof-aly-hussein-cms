using ProfAly.CMS.Domain.Common;

namespace ProfAly.CMS.Tests;

/// <summary>Validates YouTube id extraction + derived URLs (Videos module, Stage 9).</summary>
public class YouTubeHelperTests
{
    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/shorts/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/watch?list=PLx&v=dQw4w9WgXcQ&t=10s", "dQw4w9WgXcQ")]
    [InlineData("http://youtube.com/v/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("  dQw4w9WgXcQ  ", "dQw4w9WgXcQ")]
    public void TryGetVideoId_ExtractsId_FromSupportedForms(string input, string expected)
    {
        Assert.True(YouTube.TryGetVideoId(input, out var id));
        Assert.Equal(expected, id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("https://example.com/watch?v=tooLong12345")]
    [InlineData("not a url")]
    [InlineData("https://vimeo.com/123456")]
    public void TryGetVideoId_Fails_ForInvalidInput(string? input)
    {
        Assert.False(YouTube.TryGetVideoId(input, out var id));
        Assert.Equal(string.Empty, id);
    }

    [Fact]
    public void DerivedUrls_AreWellFormed()
    {
        const string id = "dQw4w9WgXcQ";
        Assert.Equal("https://img.youtube.com/vi/dQw4w9WgXcQ/hqdefault.jpg", YouTube.ThumbnailUrl(id));
        Assert.Equal("https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ", YouTube.EmbedUrl(id));
        Assert.Equal("https://www.youtube.com/watch?v=dQw4w9WgXcQ", YouTube.WatchUrl(id));
    }
}
