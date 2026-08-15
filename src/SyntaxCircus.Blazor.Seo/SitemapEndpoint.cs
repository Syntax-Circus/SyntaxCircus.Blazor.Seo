using System.Globalization;
using System.Text;
using System.Xml;

namespace SyntaxCircus.Blazor.Seo;

public sealed record SitemapEntry(string Url, DateTime LastModifiedUtc, string ChangeFrequency = "monthly", double Priority = 0.5);

public static class SitemapEndpoint
{
    /// <summary>
    /// Maps <c>/sitemap.xml</c>. <paramref name="staticEntries"/> is combined at request time with
    /// whatever <paramref name="dynamicEntriesProvider"/> resolves (if supplied) — e.g. a lookup
    /// against your product catalog. If your app has output caching configured, chain
    /// <c>.CacheOutput(...)</c> onto the returned builder yourself; this method doesn't assume it.
    /// </summary>
    public static IEndpointConventionBuilder MapSitemap(
        this IEndpointRouteBuilder routes,
        IReadOnlyList<SitemapEntry> staticEntries,
        Func<IServiceProvider, CancellationToken, Task<IReadOnlyList<SitemapEntry>>>? dynamicEntriesProvider = null)
    {
        ArgumentNullException.ThrowIfNull(routes);
        ArgumentNullException.ThrowIfNull(staticEntries);

        return routes.MapGet("/sitemap.xml", async (HttpContext context, CancellationToken cancellationToken) =>
        {
            var entries = staticEntries;
            if (dynamicEntriesProvider is not null)
            {
                var dynamicEntries = await dynamicEntriesProvider(context.RequestServices, cancellationToken).ConfigureAwait(false);
                entries = [.. staticEntries, .. dynamicEntries];
            }

            var xml = BuildSitemapXml(entries);
            return Results.Text(xml, "application/xml", Encoding.UTF8);
        })
        .WithName("SyntaxCircusSitemap");
    }

    private static string BuildSitemapXml(IReadOnlyList<SitemapEntry> entries)
    {
        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            Indent = true,
        };

        using var stream = new MemoryStream();
        using (var writer = XmlWriter.Create(stream, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

            foreach (var entry in entries)
            {
                writer.WriteStartElement("url");
                writer.WriteElementString("loc", entry.Url);
                writer.WriteElementString("lastmod", entry.LastModifiedUtc.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                writer.WriteElementString("changefreq", entry.ChangeFrequency);
                writer.WriteElementString("priority", entry.Priority.ToString("0.0", CultureInfo.InvariantCulture));
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
