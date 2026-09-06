using System.Text;

namespace SyntaxCircus.Blazor.Seo;

/// <summary>
/// Thin wrappers over <c>SyntaxCircus.AspNetCore.Common</c>'s <see cref="SearchDiscoveryEndpointExtensions"/> that
/// add the Blazor.Seo-specific conveniences this package used to implement itself: concatenating static and
/// per-request dynamic sitemap entries, and auto-appending a <c>Sitemap:</c> line to robots.txt via
/// <see cref="ISeoUrlBuilder"/>. The actual XML/text rendering, cache-control handling, and the
/// <c>SearchIndexing:BlockRobotsAndSitemap</c> kill-switch all live in AspNetCore.Common.
/// </summary>
public static class SeoEndpointExtensions
{
    /// <summary>
    /// Maps <c>/sitemap.xml</c>. <paramref name="staticEntries"/> is combined at request time with
    /// whatever <paramref name="dynamicEntriesProvider"/> resolves (if supplied) — e.g. a lookup
    /// against your product catalog. If your app has output caching configured, chain
    /// <c>.CacheOutput(...)</c> onto the returned builder yourself; this method doesn't assume it.
    /// </summary>
    public static IEndpointRouteBuilder MapSeoSitemap(
        this IEndpointRouteBuilder routes,
        IReadOnlyList<SitemapEntry> staticEntries,
        Func<IServiceProvider, CancellationToken, Task<IReadOnlyList<SitemapEntry>>>? dynamicEntriesProvider = null)
    {
        ArgumentNullException.ThrowIfNull(routes);
        ArgumentNullException.ThrowIfNull(staticEntries);

        return routes.MapSitemap(async context =>
        {
            if (dynamicEntriesProvider is null)
            {
                return staticEntries;
            }

            var dynamicEntries = await dynamicEntriesProvider(context.RequestServices, context.RequestAborted).ConfigureAwait(false);
            return (IReadOnlyList<SitemapEntry>)[.. staticEntries, .. dynamicEntries];
        });
    }

    /// <summary>Maps <c>/robots.txt</c>: allows all crawlers and points at <c>/sitemap.xml</c>.</summary>
    public static IEndpointRouteBuilder MapSeoRobotsTxt(
        this IEndpointRouteBuilder routes,
        IReadOnlyList<string>? extraDirectives = null)
    {
        ArgumentNullException.ThrowIfNull(routes);

        return routes.MapRobotsTxt(context =>
        {
            var urlBuilder = context.RequestServices.GetRequiredService<ISeoUrlBuilder>();
            var sitemapUrl = urlBuilder.AbsoluteUrl("/sitemap.xml");

            var builder = new StringBuilder();
            builder.AppendLine("User-agent: *");
            builder.AppendLine("Allow: /");

            if (extraDirectives is not null)
            {
                foreach (var directive in extraDirectives)
                {
                    builder.AppendLine(directive);
                }
            }

            builder.AppendLine($"Sitemap: {sitemapUrl}");
            return builder.ToString();
        });
    }
}
