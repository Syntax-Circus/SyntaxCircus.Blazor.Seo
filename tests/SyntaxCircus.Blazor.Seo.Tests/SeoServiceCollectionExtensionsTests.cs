namespace SyntaxCircus.Blazor.Seo.Tests;

public class SeoServiceCollectionExtensionsTests
{
    private static IConfiguration EmptyConfiguration()
        => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();

    [Fact]
    public void AddSyntaxCircusSeo_NullServices_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            SeoServiceCollectionExtensions.AddSyntaxCircusSeo(null!, EmptyConfiguration()));
    }

    [Fact]
    public void AddSyntaxCircusSeo_NullConfiguration_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();

        Should.Throw<ArgumentNullException>(() => services.AddSyntaxCircusSeo(null!));
    }

    [Fact]
    public void AddSyntaxCircusSeo_RegistersIHttpContextAccessor()
    {
        var services = new ServiceCollection();
        services.AddSyntaxCircusSeo(EmptyConfiguration());

        using var provider = services.BuildServiceProvider();

        provider.GetService<IHttpContextAccessor>().ShouldNotBeNull();
    }

    [Fact]
    public void AddSyntaxCircusSeo_ResolvesISeoUrlBuilderAsSeoUrlBuilder()
    {
        var services = new ServiceCollection();
        services.AddSyntaxCircusSeo(EmptyConfiguration());

        using var scope = services.BuildServiceProvider().CreateScope();

        scope.ServiceProvider.GetRequiredService<ISeoUrlBuilder>().ShouldBeOfType<SeoUrlBuilder>();
    }

    [Fact]
    public void UseCanonicalHost_NullApp_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => SeoServiceCollectionExtensions.UseCanonicalHost(null!));
    }

    [Fact]
    public void UseCanonicalHost_ReturnsSameApplicationBuilder()
    {
        var services = new ServiceCollection();
        services.AddSyntaxCircusSeo(EmptyConfiguration());
        var app = new ApplicationBuilder(services.BuildServiceProvider());

        var result = app.UseCanonicalHost();

        result.ShouldBeSameAs(app);
    }
}
