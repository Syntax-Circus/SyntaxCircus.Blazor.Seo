namespace SyntaxCircus.Blazor.Seo.Tests;

public class SeoEndpointExtensionsTests
{
    [Fact]
    public void MapSeoSitemap_NullRoutes_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => SeoEndpointExtensions.MapSeoSitemap(null!, []));
    }

    [Fact]
    public void MapSeoSitemap_NullStaticEntries_ThrowsArgumentNullException()
    {
        var routes = Substitute.For<IEndpointRouteBuilder>();

        Should.Throw<ArgumentNullException>(() => routes.MapSeoSitemap(null!));
    }

    [Fact]
    public void MapSeoRobotsTxt_NullRoutes_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => SeoEndpointExtensions.MapSeoRobotsTxt(null!));
    }

    private static TestServer CreateSitemapServer(
        IReadOnlyList<SitemapEntry> staticEntries,
        Func<IServiceProvider, CancellationToken, Task<IReadOnlyList<SitemapEntry>>>? dynamicEntriesProvider = null,
        Action<IServiceCollection>? configureServices = null)
        => TestServerFactory.Create(configureServices, endpoints => endpoints.MapSeoSitemap(staticEntries, dynamicEntriesProvider));

    [Fact]
    public async Task GetSitemap_ReturnsApplicationXmlContentType()
    {
        using var server = CreateSitemapServer([new SitemapEntry("https://example.com/")]);
        using var client = server.CreateClient();

        var response = await client.GetAsync(new Uri("/sitemap.xml", UriKind.Relative), TestContext.Current.CancellationToken);

        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/xml");
    }

    [Fact]
    public async Task GetSitemap_StaticEntriesOnly_ContainsExpectedUrlAndFormatting()
    {
        using var server = CreateSitemapServer([new SitemapEntry("https://example.com/page", new DateOnly(2026, 3, 5), SitemapChangeFrequency.Weekly, 0.8)]);
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
        var staticEntries = new List<SitemapEntry> { new("https://example.com/static") };
        using var server = CreateSitemapServer(staticEntries, (_, _) =>
            Task.FromResult<IReadOnlyList<SitemapEntry>>([new SitemapEntry("https://example.com/dynamic")]));
        using var client = server.CreateClient();

        var body = await client.GetStringAsync(new Uri("/sitemap.xml", UriKind.Relative), TestContext.Current.CancellationToken);

        body.ShouldContain("https://example.com/static");
        body.ShouldContain("https://example.com/dynamic");
    }

    [Fact]
    public async Task GetSitemap_EmptyEntries_ReturnsEmptyUrlset()
    {
        using var server = CreateSitemapServer([]);
        using var client = server.CreateClient();

        var body = await client.GetStringAsync(new Uri("/sitemap.xml", UriKind.Relative), TestContext.Current.CancellationToken);

        body.ShouldNotContain("<url>");
        body.ShouldContain("urlset");
    }

    [Fact]
    public async Task GetSitemap_DelegatesToAspNetCoreCommon_BlockRobotsAndSitemapReturnsNotFound()
    {
        using var server = CreateSitemapServer(
            [new SitemapEntry("https://example.com/")],
            configureServices: services => services.Configure<SearchIndexingOptions>(o => o.BlockRobotsAndSitemap = true));
        using var client = server.CreateClient();

        var response = await client.GetAsync(new Uri("/sitemap.xml", UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
    }

    private static TestServer CreateRobotsServer(
        IReadOnlyList<string>? extraDirectives = null,
        Action<IServiceCollection>? extraServices = null)
    {
        var urlBuilder = Substitute.For<ISeoUrlBuilder>();
        urlBuilder.AbsoluteUrl("/sitemap.xml").Returns("https://example.com/sitemap.xml");

        return TestServerFactory.Create(
            services =>
            {
                services.AddSingleton(urlBuilder);
                extraServices?.Invoke(services);
            },
            endpoints => endpoints.MapSeoRobotsTxt(extraDirectives));
    }

    [Fact]
    public async Task GetRobotsTxt_ReturnsTextPlainContentType()
    {
        using var server = CreateRobotsServer();
        using var client = server.CreateClient();

        var response = await client.GetAsync(new Uri("/robots.txt", UriKind.Relative), TestContext.Current.CancellationToken);

        response.Content.Headers.ContentType!.MediaType.ShouldBe("text/plain");
    }

    [Fact]
    public async Task GetRobotsTxt_ContainsUserAgentAndAllowDirectives()
    {
        using var server = CreateRobotsServer();
        using var client = server.CreateClient();

        var body = await client.GetStringAsync(new Uri("/robots.txt", UriKind.Relative), TestContext.Current.CancellationToken);

        body.ShouldContain("User-agent: *");
        body.ShouldContain("Allow: /");
    }

    [Fact]
    public async Task GetRobotsTxt_ContainsSitemapLineFromUrlBuilder()
    {
        using var server = CreateRobotsServer();
        using var client = server.CreateClient();

        var body = await client.GetStringAsync(new Uri("/robots.txt", UriKind.Relative), TestContext.Current.CancellationToken);

        body.ShouldContain("Sitemap: https://example.com/sitemap.xml");
    }

    [Fact]
    public async Task GetRobotsTxt_ExtraDirectivesAppendedInOrder()
    {
        using var server = CreateRobotsServer(["Disallow: /admin", "Crawl-delay: 5"]);
        using var client = server.CreateClient();

        var body = await client.GetStringAsync(new Uri("/robots.txt", UriKind.Relative), TestContext.Current.CancellationToken);

        var disallowIndex = body.IndexOf("Disallow: /admin", StringComparison.Ordinal);
        var crawlDelayIndex = body.IndexOf("Crawl-delay: 5", StringComparison.Ordinal);
        var sitemapIndex = body.IndexOf("Sitemap:", StringComparison.Ordinal);

        disallowIndex.ShouldBeGreaterThanOrEqualTo(0);
        crawlDelayIndex.ShouldBeGreaterThan(disallowIndex);
        sitemapIndex.ShouldBeGreaterThan(crawlDelayIndex);
    }

    [Fact]
    public async Task GetRobotsTxt_DelegatesToAspNetCoreCommon_BlockRobotsAndSitemapReturnsDenyAllBody()
    {
        using var server = CreateRobotsServer(
            extraServices: services => services.Configure<SearchIndexingOptions>(o => o.BlockRobotsAndSitemap = true));
        using var client = server.CreateClient();

        var body = await client.GetStringAsync(new Uri("/robots.txt", UriKind.Relative), TestContext.Current.CancellationToken);

        body.ShouldBe(SearchIndexingOptions.DisallowAllRobotsTxt);
    }
}
