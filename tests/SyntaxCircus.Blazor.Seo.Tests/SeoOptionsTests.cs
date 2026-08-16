namespace SyntaxCircus.Blazor.Seo.Tests;

public class SeoOptionsTests
{
    [Fact]
    public void Defaults_AreExpected()
    {
        var options = new SeoOptions();

        options.BaseUrl.ShouldBe(string.Empty);
        options.SiteName.ShouldBe(string.Empty);
        options.DefaultDescription.ShouldBe(string.Empty);
        options.DefaultOgImage.ShouldBe(string.Empty);
        options.DefaultLocale.ShouldBe("en_US");
        options.TwitterHandle.ShouldBeNull();
        options.LogoUrl.ShouldBeNull();
        options.SameAs.ShouldBeEmpty();
    }

    [Fact]
    public void SectionName_IsSeo()
    {
        SeoOptions.SectionName.ShouldBe("Seo");
    }
}
