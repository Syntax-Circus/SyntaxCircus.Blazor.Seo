# SyntaxCircus.Blazor.Seo

[![Build](https://github.com/Syntax-Circus/SyntaxCircus.Blazor.Seo/actions/workflows/build.yml/badge.svg)](https://github.com/Syntax-Circus/SyntaxCircus.Blazor.Seo/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.txt)

SEO building blocks for Blazor Server marketing sites — meta/OG/Twitter tags, typed Schema.org JSON-LD, a canonical URL builder, a generic sitemap.xml/robots.txt endpoint, and canonical-host redirect middleware.

> **No support guaranteed.** Published as-is and maintained on a best-effort basis. Issues and PRs are welcome, but there's no SLA — fork it or vendor what you need if that's not enough.

## Setup

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSyntaxCircusSeo(builder.Configuration); // binds "Seo" and "Support"

var app = builder.Build();
app.UseCanonicalHost(); // optional — 301s non-canonical hosts to Seo:BaseUrl's host

app.MapSitemap(
    staticEntries:
    [
        new SitemapEntry(builder.Configuration["Seo:BaseUrl"] + "/", DateTime.UtcNow, "weekly", 1.0),
        new SitemapEntry(builder.Configuration["Seo:BaseUrl"] + "/about", DateTime.UtcNow),
    ],
    dynamicEntriesProvider: async (services, ct) =>
    {
        var catalog = services.GetRequiredService<IMyCatalogService>();
        return catalog.GetEntries().Select(e => new SitemapEntry($"{baseUrl}/items/{e.Slug}", e.UpdatedAt)).ToArray();
    });

app.MapRobotsTxt();
```

## Configuration

| Section | Purpose |
|---|---|
| `Seo` | `BaseUrl`, `SiteName`, `DefaultDescription`, `DefaultOgImage`, `DefaultLocale`, `TwitterHandle`, `LogoUrl`, `SameAs` |
| `Support` | Optional site support/contact info — `Email`, `IssueTrackerUrl`, `IssueTemplateUrl`, `OwnerContactUrl`, `OwnerDisplayName` |

## Per-page meta tags

```razor
<SeoHead Title="Pricing" Description="Plans and pricing." RelativeUrl="/pricing" />
<JsonLd Data="@(new OrganizationSchema(Name: "Acme", Url: "https://acme.example", Logo: "https://acme.example/logo.png"))" />
```

`SeoHead` fills in description, canonical link, robots directives, and Open Graph/Twitter tags from `SeoOptions` plus whatever you override per-page. `JsonLd` serializes any typed schema record from `Schemas.cs` (`OrganizationSchema`, `WebSiteSchema`, `PersonSchema`, `CreativeWorkSchema`, `SoftwareApplicationSchema`, `BookSchema`, `BreadcrumbListSchema`, `FaqPageSchema`) — or your own POCO — as a `<script type="application/ld+json">` block. Use as many `<JsonLd>` instances on one page as you need.

## Notes

- `MapSitemap`/`MapRobotsTxt` don't assume output caching is configured — if your app has `AddOutputCache()`/`UseOutputCache()` set up, chain `.CacheOutput(...)` onto the returned endpoint builder yourself.
- `ISeoUrlBuilder` (injectable) resolves relative URLs against `Seo:BaseUrl` and can compute the canonical URL for the current request.

## Contributing

Issues and pull requests are welcome:
- Keep changes focused, with a clear description of the behavior change.
- Match the existing code style (see `.editorconfig`).
- Call out any breaking changes to the public API in your PR description.

## License

MIT — see [LICENSE.txt](LICENSE.txt).
