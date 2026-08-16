namespace SyntaxCircus.Blazor.Seo.Tests;

public class SitemapEndpointTests
{
    [Fact]
    public void MapSitemap_NullRoutes_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => SitemapEndpoint.MapSitemap(null!, []));
    }

    [Fact]
    public void MapSitemap_NullStaticEntries_ThrowsArgumentNullException()
    {
        var routes = Substitute.For<IEndpointRouteBuilder>();

        Should.Throw<ArgumentNullException>(() => routes.MapSitemap(null!));
    }

    private static TestServer CreateServer(
        IReadOnlyList<SitemapEntry> staticEntries,
        Func<IServiceProvider, CancellationToken, Task<IReadOnlyList<SitemapEntry>>>? dynamicEntriesProvider = null)
        => TestServerFactory.Create(null, endpoints => endpoints.MapSitemap(staticEntries, dynamicEntriesProvider));

    [Fact]
    public async Task GetSitemap_ReturnsApplicationXmlContentType()
    {
        using var server = CreateServer([new SitemapEntry("https://example.com/", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc))]);
        using var client = server.CreateClient();

        var response = await client.GetAsync(new Uri("/sitemap.xml", UriKind.Relative), TestContext.Current.CancellationToken);

        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/xml");
    }

    [Fact]
    public async Task GetSitemap_StaticEntriesOnly_ContainsExpectedUrlAndFormatting()
    {
        using var server = CreateServer([new SitemapEntry("https://example.com/page", new DateTime(2026, 3, 5, 0, 0, 0, DateTimeKind.Utc), "weekly", 0.8)]);
        using var client = server.CreateClient();

        var body = await client.GetStringAsync(new Uri("/sitemap.xml", UriKind.Relative), TestContext.Current.CancellationToken);

        body.ShouldContain("<loc>https://example.com/page</loc>");
        body.ShouldContain("<lastmod>2026-03-05</lastmod>");
        body.ShouldContain("<changefreq>weekly</changefreq>");
        body.ShouldContain("<priority>0.8</priority>");
    }

    [Fact]
    public async Task GetSitemap_WithDynamicEntriesProvider_ConcatenatesStaticAndDynamic()
    {
        var staticEntries = new List<SitemapEntry> { new("https://example.com/static", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)) };
        using var server = CreateServer(staticEntries, (_, _) =>
            Task.FromResult<IReadOnlyList<SitemapEntry>>([new SitemapEntry("https://example.com/dynamic", new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc))]));
        using var client = server.CreateClient();

        var body = await client.GetStringAsync(new Uri("/sitemap.xml", UriKind.Relative), TestContext.Current.CancellationToken);

        body.ShouldContain("https://example.com/static");
        body.ShouldContain("https://example.com/dynamic");
    }

    [Fact]
    public async Task GetSitemap_EmptyEntries_ReturnsEmptyUrlset()
    {
        using var server = CreateServer([]);
        using var client = server.CreateClient();

        var body = await client.GetStringAsync(new Uri("/sitemap.xml", UriKind.Relative), TestContext.Current.CancellationToken);

        body.ShouldNotContain("<url>");
        body.ShouldContain("urlset");
    }
}
