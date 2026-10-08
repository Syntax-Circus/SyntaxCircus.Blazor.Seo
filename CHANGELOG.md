# Changelog

All notable changes to this project are documented here. Format loosely follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## 0.1.5

### Changed

- `ISeoUrlBuilder` is registered with `TryAddScoped`, so a consumer's own registration wins regardless of order
  (before or after `AddSyntaxCircusSeo`). See the multi-host note in the README.
- `SyntaxCircus.AspNetCore.Common` floor raised to 0.1.16.

## 0.1.4

### BREAKING CHANGES

- **Now depends on `SyntaxCircus.AspNetCore.Common` (>= 0.1.11)** for sitemap.xml, robots.txt, and canonical-host
  redirect. These primitives are no longer reimplemented in this package.
- **Removed types**: `SitemapEndpoint`, `RobotsTxtEndpoint`, `CanonicalHostMiddleware`, and this package's own
  `SitemapEntry` record are gone.
  - `SitemapEndpoint.MapSitemap` / `RobotsTxtEndpoint.MapRobotsTxt` are replaced by
    `SeoEndpointExtensions.MapSeoSitemap` / `MapSeoRobotsTxt`, which delegate to AspNetCore.Common's
    `MapSitemap`/`MapRobotsTxt` under the hood.
  - Callers now use `SyntaxCircus.AspNetCore.Common.SitemapEntry` (with `SitemapChangeFrequency` enum and
    `DateOnly? LastModified`) instead of this package's old `SitemapEntry(Url, LastModifiedUtc, ChangeFrequency:
    string, Priority)` record.
  - **Not just a type-shape change — the defaults produce sparser XML.** The old `SitemapEntry` defaulted
    `ChangeFrequency` to `"monthly"` and `Priority` to `0.5`, both non-null and always rendered. The new
    `SyntaxCircus.AspNetCore.Common.SitemapEntry` defaults `ChangeFrequency`/`Priority` to `null`, and its
    sitemap XML writer *omits* `<changefreq>`/`<priority>` entirely when they're null (`LastModified` works
    the same way — omitted when null). A mechanical port of `new SitemapEntry(url, date)` calls — dropping
    the old positional `ChangeFrequency`/`Priority` args because the new type made them optional too — silently
    produces sitemap entries missing those elements, not just a recompiled call site. Pass
    `SitemapChangeFrequency`/`Priority` explicitly if you want them present in the output.
  - `SeoServiceCollectionExtensions.UseCanonicalHost` is removed. Use the new
    `SeoServiceCollectionExtensions.UseSyntaxCircusSeo(app)`, which calls AspNetCore.Common's
    `UseCanonicalHostRedirect()` and `UseSearchIndexingHeaders()`.
  - **`MapSeoSitemap`/`MapSeoRobotsTxt` return `IEndpointRouteBuilder`, not a per-route builder — nothing is
    chainable off them anymore, and the named-endpoint registration is gone.** The old `SitemapEndpoint`/
    `RobotsTxtEndpoint` called `.WithName("SyntaxCircusSitemap")`/`.WithName("SyntaxCircusRobotsTxt")` on the
    endpoint they mapped, enabling `LinkGenerator`/`Url.RouteUrl(...)` lookups; AspNetCore.Common's
    `MapSitemap`/`MapRobotsTxt` don't do this and don't expose the per-route `IEndpointConventionBuilder` needed
    to add it back (they only use it internally for `.AllowAnonymous()`, then return the `endpoints` parameter
    itself). This also means `.CacheOutput(...)` can no longer be chained onto the result the way the README
    used to describe. Use the new `cacheDuration` parameter on both methods for response caching (sets
    `Cache-Control: public, max-age=...` directly) instead, and reference `/sitemap.xml`/`/robots.txt` by
    literal path rather than by route name if you need to link to them.
- **Canonical-host redirect semantics changed from deny-by-default to allow-by-default.** The old
  `CanonicalHostMiddleware` redirected *any* request host that didn't match `Seo:BaseUrl`'s host, except a
  hardcoded skip-list (localhost/127.\*/hosts containing "internal"). AspNetCore.Common's
  `UseCanonicalHostRedirect()` only redirects hosts you explicitly list in `CanonicalHost:LegacyHosts` — every
  other host, including ones you didn't anticipate, passes through untouched. If you relied on the old
  deny-by-default behavior, add your legacy/alternate hostnames to `CanonicalHost:LegacyHosts` explicitly; a
  bare `CanonicalHost:CanonicalHost` setting with no `LegacyHosts` entries will no longer redirect anything.
- `AddSyntaxCircusSeo(services, configuration)` now also calls `AddCanonicalHostRedirect(configuration)` and
  `AddSearchIndexing(configuration)` internally, binding the `"CanonicalHost"` and `"SearchIndexing"`
  configuration sections in addition to `"Seo"` and `"Support"`.

### Unchanged

- `SeoHead`, `JsonLd`, `Schemas.cs`, `ISeoUrlBuilder`/`SeoUrlBuilder`, `SeoOptions`, and `SiteSupportOptions` are
  untouched — this package continues to own the Blazor presentation layer (meta/OG/Twitter tags, JSON-LD,
  canonical URL resolution).
