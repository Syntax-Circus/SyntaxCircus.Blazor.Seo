# Changelog

All notable changes to this project are documented here. Format loosely follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## Unreleased

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
  - `SeoServiceCollectionExtensions.UseCanonicalHost` is removed. Use the new
    `SeoServiceCollectionExtensions.UseSyntaxCircusSeo(app)`, which calls AspNetCore.Common's
    `UseCanonicalHostRedirect()` and `UseSearchIndexingHeaders()`.
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
