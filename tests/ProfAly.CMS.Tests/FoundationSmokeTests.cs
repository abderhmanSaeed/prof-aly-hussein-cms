using ProfAly.CMS.Domain.Common;

namespace ProfAly.CMS.Tests;

/// <summary>
/// Skeleton-phase smoke tests. Confirms the solution wires up and the shared
/// culture constants behave. Real service/integration tests arrive from Stage 9.
/// </summary>
public class FoundationSmokeTests
{
    [Fact]
    public void SupportedCultures_AreArEnFr_WithArabicDefault()
    {
        Assert.Equal(new[] { "ar", "en", "fr" }, SupportedCultures.All);
        Assert.Equal("ar", SupportedCultures.Default);
    }

    [Theory]
    [InlineData("ar", true)]
    [InlineData("en", true)]
    [InlineData("fr", true)]
    [InlineData("de", false)]
    [InlineData(null, false)]
    public void IsSupported_ValidatesCulture(string? culture, bool expected)
    {
        Assert.Equal(expected, SupportedCultures.IsSupported(culture));
    }

    [Fact]
    public void Arabic_IsRightToLeft()
    {
        Assert.True(SupportedCultures.IsRightToLeft("ar"));
        Assert.False(SupportedCultures.IsRightToLeft("en"));
    }
}
