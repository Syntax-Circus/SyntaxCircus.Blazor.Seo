namespace SyntaxCircus.Blazor.Seo;

public static class SeoServiceCollectionExtensions
{
    /// <summary>
    /// Registers Blazor.Seo's own services (<see cref="SeoOptions"/>, <see cref="SiteSupportOptions"/>,
    /// <see cref="ISeoUrlBuilder"/>) plus, via <c>SyntaxCircus.AspNetCore.Common</c>, canonical-host-redirect
    /// (<c>"CanonicalHost"</c>) and search-indexing header (<c>"SearchIndexing"</c>) configuration — one call
    /// wires up the whole stack. Pair with <see cref="UseSyntaxCircusSeo"/> in the request pipeline.
    /// </summary>
    public static IServiceCollection AddSyntaxCircusSeo(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddHttpContextAccessor();
        services.Configure<SeoOptions>(configuration.GetSection(SeoOptions.SectionName));
        services.Configure<SiteSupportOptions>(configuration.GetSection(SiteSupportOptions.SectionName));
        services.AddScoped<ISeoUrlBuilder, SeoUrlBuilder>();
        services.AddCanonicalHostRedirect(configuration);
        services.AddSearchIndexing(configuration);
        return services;
    }

    /// <summary>
    /// Applies the canonical-host redirect and search-indexing (<c>X-Robots-Tag</c>) middleware registered by
    /// <see cref="AddSyntaxCircusSeo"/> — equivalent to calling AspNetCore.Common's
    /// <c>UseCanonicalHostRedirect()</c> then <c>UseSearchIndexingHeaders()</c> yourself.
    /// </summary>
    public static IApplicationBuilder UseSyntaxCircusSeo(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseCanonicalHostRedirect();
        app.UseSearchIndexingHeaders();
        return app;
    }
}
