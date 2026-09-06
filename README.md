# SyntaxCircus.Blazor.Seo

[![Build](https://github.com/Syntax-Circus/SyntaxCircus.Blazor.Seo/actions/workflows/build.yml/badge.svg)](https://github.com/Syntax-Circus/SyntaxCircus.Blazor.Seo/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/SyntaxCircus.Blazor.Seo.svg)](https://www.nuget.org/packages/SyntaxCircus.Blazor.Seo)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.txt)

SEO building blocks for Blazor Server marketing sites — meta/OG/Twitter tags, typed Schema.org JSON-LD, and a canonical URL builder. Sitemap.xml, robots.txt, and canonical-host redirect are thin wrappers over [`SyntaxCircus.AspNetCore.Common`](https://www.nuget.org/packages/SyntaxCircus.AspNetCore.Common), which this package depends on rather than reimplementing.

> **No support guaranteed.** Published as-is and maintained on a best-effort basis. Issues and PRs are welcome, but there's no SLA — fork it or vendor what you need if that's not enough.

> **Breaking change from previous versions:** `SitemapEndpoint`, `RobotsTxtEndpoint`, and `CanonicalHostMiddleware` are gone, replaced by `SeoEndpointExtensions` (`MapSeoSitemap`/`MapSeoRobotsTxt`) and AspNetCore.Common's canonical-host redirect. The canonical-host redirect is now an **allow-list** (`CanonicalHost:LegacyHosts`), not a deny-by-default redirect of every mismatched host. See [CHANGELOG.md](CHANGELOG.md) for details.

## Setup

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSyntaxCircusSeo(builder.Configuration); // binds "Seo", "Support", "CanonicalHost", "SearchIndexing"

var app = builder.Build();
app.UseSyntaxCircusSeo(); // canonical-host redirect + X-Robots-Tag search-indexing headers

app.MapSeoSitemap(
    staticEntries:
    [
        new SitemapEntry(builder.Configuration["Seo:BaseUrl"] + "/", DateOnly.FromDateTime(DateTime.UtcNow), SitemapChangeFrequency.Weekly, 1.0),
        new SitemapEntry(builder.Configuration["Seo:BaseUrl"] + "/about", DateOnly.FromDateTime(DateTime.UtcNow)),
    ],
    dynamicEntriesProvider: async (services, ct) =>
    {
        var catalog = services.GetRequiredService<IMyCatalogService>();
        return catalog.GetEntries().Select(e => new SitemapEntry($"{baseUrl}/items/{e.Slug}", DateOnly.FromDateTime(e.UpdatedAt))).ToArray();
    });

app.MapSeoRobotsTxt();
```

`SitemapEntry` and `SitemapChangeFrequency` come from `SyntaxCircus.AspNetCore.Common` — see that package for the full sitemap/robots.txt/canonical-host/search-indexing option surface (`CanonicalHost:CanonicalHost`, `CanonicalHost:LegacyHosts`, `SearchIndexing:BlockRobotsAndSitemap`, etc.).

## Configuration

| Section | Purpose | Owned by |
|---|---|---|
| `Seo` | `BaseUrl`, `SiteName`, `DefaultDescription`, `DefaultOgImage`, `DefaultLocale`, `TwitterHandle`, `LogoUrl`, `SameAs` | Blazor.Seo |
| `Support` | Optional site support/contact info — `Email`, `IssueTrackerUrl`, `IssueTemplateUrl`, `OwnerContactUrl`, `OwnerDisplayName` | Blazor.Seo |
| `CanonicalHost` | `CanonicalHost`, `LegacyHosts`, `ForceHttps`, `Permanent` | AspNetCore.Common |
| `SearchIndexing` | `BlockPageMetadata`, `RobotsDirective`, `BlockRobotsAndSitemap`, `ExcludedPaths` | AspNetCore.Common |

## Per-page meta tags

```razor
<SeoHead
    Title="Pricing"
    Description="Plans and pricing."
    RelativeUrl="/pricing"
    StructuredData="PageSchemas" />

@code {
    private IReadOnlyList<object> PageSchemas =>
    [
        new OrganizationSchema(
            Name: "Acme",
            Url: "https://acme.example",
            Logo: "https://acme.example/logo.png"),
        new BreadcrumbListSchema(
            [new BreadcrumbItem(1, "Home", "https://acme.example/"), new BreadcrumbItem(2, "Pricing", "https://acme.example/pricing")]),
    ];
}
```

`SeoHead` sets the document title and fills in description, canonical link, robots directives, and Open Graph/Twitter tags from `SeoOptions` plus whatever you override per-page. Its optional `StructuredData` parameter renders each supplied object as a JSON-LD block in the document head. `JsonLd` remains available when you prefer to place a single schema directly; both accept typed records from `Schemas.cs` (`OrganizationSchema`, `WebSiteSchema`, `PersonSchema`, `CreativeWorkSchema`, `SoftwareApplicationSchema`, `BookSchema`, `BreadcrumbListSchema`, `FaqPageSchema`) or your own POCO.

## Notes

- `MapSeoSitemap`/`MapSeoRobotsTxt` return `IEndpointRouteBuilder` — the same builder you called them on, passed straight through from AspNetCore.Common's `MapSitemap`/`MapRobotsTxt`, **not** a per-route `IEndpointConventionBuilder`. Nothing can be chained onto the result (no `.CacheOutput(...)`, `.WithName(...)`, etc.) — that per-route builder isn't exposed by AspNetCore.Common's API. Pass the `cacheDuration` parameter instead to set a `Cache-Control: public, max-age=...` response header directly, e.g. `app.MapSeoSitemap(entries, cacheDuration: TimeSpan.FromHours(1))`.
- Neither endpoint is registered under a route name (no `LinkGenerator`/`Url.RouteUrl(...)` support) — the previous versions' `.WithName("SyntaxCircusSitemap")`/`.WithName("SyntaxCircusRobotsTxt")` calls had nowhere to attach once mapping moved into AspNetCore.Common. Reference `/sitemap.xml`/`/robots.txt` by literal path if you need to link to them.
- `ISeoUrlBuilder` (injectable) resolves relative URLs against `Seo:BaseUrl` and can compute the canonical URL for the current request. `MapSeoRobotsTxt` uses it to auto-append the `Sitemap:` line.
- `SearchIndexing:BlockRobotsAndSitemap` (AspNetCore.Common) makes `MapSeoRobotsTxt` return a deny-all body and `MapSeoSitemap` return 404 — a single kill-switch for taking a site out of search indexing.

## Contributing

Issues and pull requests are welcome:
- Keep changes focused, with a clear description of the behavior change.
- Match the existing code style (see `.editorconfig`).
- Call out any breaking changes to the public API in your PR description.

## License

MIT — see [LICENSE.txt](LICENSE.txt).
