namespace SyntaxCircus.Blazor.Seo.Tests;

public class RobotsTxtEndpointTests
{
    [Fact]
    public void MapRobotsTxt_NullRoutes_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => RobotsTxtEndpoint.MapRobotsTxt(null!));
    }

    private static TestServer CreateServer(IReadOnlyList<string>? extraDirectives = null)
    {
        var urlBuilder = Substitute.For<ISeoUrlBuilder>();
        urlBuilder.AbsoluteUrl("/sitemap.xml").Returns("https://example.com/sitemap.xml");

        return TestServerFactory.Create(
            services => services.AddSingleton(urlBuilder),
            endpoints => endpoints.MapRobotsTxt(extraDirectives));
    }

    [Fact]
    public async Task GetRobotsTxt_ReturnsTextPlainContentType()
    {
        using var server = CreateServer();
        using var client = server.CreateClient();

        var response = await client.GetAsync(new Uri("/robots.txt", UriKind.Relative), TestContext.Current.CancellationToken);

        response.Content.Headers.ContentType!.MediaType.ShouldBe("text/plain");
    }

    [Fact]
    public async Task GetRobotsTxt_ContainsUserAgentAndAllowDirectives()
    {
        using var server = CreateServer();
        using var client = server.CreateClient();

        var body = await client.GetStringAsync(new Uri("/robots.txt", UriKind.Relative), TestContext.Current.CancellationToken);

        body.ShouldContain("User-agent: *");
        body.ShouldContain("Allow: /");
    }

    [Fact]
    public async Task GetRobotsTxt_ContainsSitemapLineFromUrlBuilder()
    {
        using var server = CreateServer();
        using var client = server.CreateClient();

        var body = await client.GetStringAsync(new Uri("/robots.txt", UriKind.Relative), TestContext.Current.CancellationToken);

        body.ShouldContain("Sitemap: https://example.com/sitemap.xml");
    }

    [Fact]
    public async Task GetRobotsTxt_ExtraDirectivesAppendedInOrder()
    {
        using var server = CreateServer(["Disallow: /admin", "Crawl-delay: 5"]);
        using var client = server.CreateClient();

        var body = await client.GetStringAsync(new Uri("/robots.txt", UriKind.Relative), TestContext.Current.CancellationToken);

        var disallowIndex = body.IndexOf("Disallow: /admin", StringComparison.Ordinal);
        var crawlDelayIndex = body.IndexOf("Crawl-delay: 5", StringComparison.Ordinal);
        var sitemapIndex = body.IndexOf("Sitemap:", StringComparison.Ordinal);

        disallowIndex.ShouldBeGreaterThanOrEqualTo(0);
        crawlDelayIndex.ShouldBeGreaterThan(disallowIndex);
        sitemapIndex.ShouldBeGreaterThan(crawlDelayIndex);
    }
}
