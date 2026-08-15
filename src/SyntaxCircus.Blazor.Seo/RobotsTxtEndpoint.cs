using System.Text;

namespace SyntaxCircus.Blazor.Seo;

public static class RobotsTxtEndpoint
{
    /// <summary>Maps <c>/robots.txt</c>: allows all crawlers and points at <c>/sitemap.xml</c>.</summary>
    public static IEndpointConventionBuilder MapRobotsTxt(
        this IEndpointRouteBuilder routes,
        IReadOnlyList<string>? extraDirectives = null)
    {
        ArgumentNullException.ThrowIfNull(routes);

        return routes.MapGet("/robots.txt", (ISeoUrlBuilder urlBuilder) =>
        {
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
            return Results.Text(builder.ToString(), "text/plain", Encoding.UTF8);
        })
        .WithName("SyntaxCircusRobotsTxt");
    }
}
