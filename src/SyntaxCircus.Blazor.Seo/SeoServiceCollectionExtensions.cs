namespace SyntaxCircus.Blazor.Seo;

public static class SeoServiceCollectionExtensions
{
    public static IServiceCollection AddSyntaxCircusSeo(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddHttpContextAccessor();
        services.Configure<SeoOptions>(configuration.GetSection(SeoOptions.SectionName));
        services.Configure<SiteSupportOptions>(configuration.GetSection(SiteSupportOptions.SectionName));
        services.AddScoped<ISeoUrlBuilder, SeoUrlBuilder>();
        return services;
    }

    /// <summary>Redirects non-canonical hosts to <see cref="SeoOptions.BaseUrl"/>'s host. Skips localhost/loopback/*.internal.</summary>
    public static IApplicationBuilder UseCanonicalHost(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<CanonicalHostMiddleware>();
    }
}
