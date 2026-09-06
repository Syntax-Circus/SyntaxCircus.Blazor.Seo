using System.Text;

namespace SyntaxCircus.Blazor.Seo;

/// <summary>
/// Thin wrappers over <c>SyntaxCircus.AspNetCore.Common</c>'s <see cref="SearchDiscoveryEndpointExtensions"/> that
/// add the Blazor.Seo-specific conveniences this package used to implement itself: concatenating static and
/// per-request dynamic sitemap entries, and auto-appending a <c>Sitemap:</c> line to robots.txt via
/// <see cref="ISeoUrlBuilder"/>. The actual XML/text rendering, cache-control handling, and the
/// <c>SearchIndexing:BlockRobotsAndSitemap</c> kill-switch all live in AspNetCore.Common.
/// </summary>
/// <remarks>
/// Both methods just delegate to and return whatever AspNetCore.Common's <c>MapSitemap</c>/<c>MapRobotsTxt</c>
/// return — the same <see cref="IEndpointRouteBuilder"/> passed in, not a per-route
/// <c>IEndpointConventionBuilder</c>. AspNetCore.Common doesn't expose the per-route builder from those methods
/// (it only uses it internally to call <c>.AllowAnonymous()</c>), so nothing here can be chained with
/// <c>.CacheOutput(...)</c>, <c>.WithName(...)</c>, etc. the way the old per-package <c>SitemapEndpoint</c>/
/// <c>RobotsTxtEndpoint</c> could. Use each method's <c>cacheDuration</c> parameter for response caching
/// instead — it sets a <c>Cache-Control: public, max-age=...</c> header directly.
/// </remarks>
public static class SeoEndpointExtensions
{
    /// <summary>
    /// Maps <c>/sitemap.xml</c>. <paramref name="staticEntries"/> is combined at request time with
    /// whatever <paramref name="dynamicEntriesProvider"/> resolves (if supplied) — e.g. a lookup
    /// against your product catalog. Pass <paramref name="cacheDuration"/> to set a
    /// <c>Cache-Control: public, max-age=...</c> response header (AspNetCore.Common's built-in mechanism);
    /// the returned <see cref="IEndpointRouteBuilder"/> is not a per-route builder, so
    /// <c>.CacheOutput(...)</c> can't be chained onto it.
    /// </summary>
    public static IEndpointRouteBuilder MapSeoSitemap(
        this IEndpointRouteBuilder routes,
        IReadOnlyList<SitemapEntry> staticEntries,
        Func<IServiceProvider, CancellationToken, Task<IReadOnlyList<SitemapEntry>>>? dynamicEntriesProvider = null,
        TimeSpan? cacheDuration = null)
    {
        ArgumentNullException.ThrowIfNull(routes);
        ArgumentNullException.ThrowIfNull(staticEntries);

        return routes.MapSitemap(
            async context =>
            {
                if (dynamicEntriesProvider is null)
                {
                    return staticEntries;
                }

                var dynamicEntries = await dynamicEntriesProvider(context.RequestServices, context.RequestAborted).ConfigureAwait(false);
                return (IReadOnlyList<SitemapEntry>)[.. staticEntries, .. dynamicEntries];
            },
            cacheDuration: cacheDuration);
    }

    /// <summary>
    /// Maps <c>/robots.txt</c>: allows all crawlers and points at <c>/sitemap.xml</c>. Pass
    /// <paramref name="cacheDuration"/> to set a <c>Cache-Control: public, max-age=...</c> response header;
    /// the returned <see cref="IEndpointRouteBuilder"/> is not a per-route builder, so <c>.CacheOutput(...)</c>
    /// can't be chained onto it.
    /// </summary>
    public static IEndpointRouteBuilder MapSeoRobotsTxt(
        this IEndpointRouteBuilder routes,
        IReadOnlyList<string>? extraDirectives = null,
        TimeSpan? cacheDuration = null)
    {
        ArgumentNullException.ThrowIfNull(routes);

        return routes.MapRobotsTxt(
            context =>
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
            },
            cacheDuration: cacheDuration);
    }
}
