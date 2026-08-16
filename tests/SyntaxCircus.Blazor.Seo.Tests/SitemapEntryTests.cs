namespace SyntaxCircus.Blazor.Seo.Tests;

public class SitemapEntryTests
{
    [Fact]
    public void Defaults_ChangeFrequencyAndPriority()
    {
        var entry = new SitemapEntry("https://example.com/", DateTime.UtcNow);

        entry.ChangeFrequency.ShouldBe("monthly");
        entry.Priority.ShouldBe(0.5);
    }

    [Fact]
    public void Ctor_SetsAllProperties()
    {
        var now = DateTime.UtcNow;
        var entry = new SitemapEntry("https://example.com/page", now, "weekly", 0.8);

        entry.Url.ShouldBe("https://example.com/page");
        entry.LastModifiedUtc.ShouldBe(now);
        entry.ChangeFrequency.ShouldBe("weekly");
        entry.Priority.ShouldBe(0.8);
    }
}
