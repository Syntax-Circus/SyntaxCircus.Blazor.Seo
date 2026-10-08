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
    public void AddSyntaxCircusSeo_KeepsBuilderRegisteredBeforeIt_ResolvesConsumerBuilder()
    {
        var stub = Substitute.For<ISeoUrlBuilder>();
        var services = new ServiceCollection();
        services.AddScoped(_ => stub);
        services.AddSyntaxCircusSeo(EmptyConfiguration());

        using var scope = services.BuildServiceProvider().CreateScope();

        scope.ServiceProvider.GetRequiredService<ISeoUrlBuilder>().ShouldBeSameAs(stub);
    }

    [Fact]
    public void AddSyntaxCircusSeo_KeepsBuilderRegisteredAfterIt_ResolvesConsumerBuilder()
    {
        var stub = Substitute.For<ISeoUrlBuilder>();
        var services = new ServiceCollection();
        services.AddSyntaxCircusSeo(EmptyConfiguration());
        services.AddScoped(_ => stub);

        using var scope = services.BuildServiceProvider().CreateScope();

        scope.ServiceProvider.GetRequiredService<ISeoUrlBuilder>().ShouldBeSameAs(stub);
    }

    [Fact]
    public void AddSyntaxCircusSeo_WiresUpCanonicalHostOptionsFromConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CanonicalHost:CanonicalHost"] = "example.com",
                ["CanonicalHost:LegacyHosts:0"] = "old.example.com",
            })
            .Build();
        var services = new ServiceCollection();
        services.AddSyntaxCircusSeo(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<CanonicalHostOptions>>().Value;

        options.CanonicalHost.ShouldBe("example.com");
        options.LegacyHosts.ShouldContain("old.example.com");
    }

    [Fact]
    public void AddSyntaxCircusSeo_WiresUpSearchIndexingOptionsFromConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SearchIndexing:BlockPageMetadata"] = "true",
            })
            .Build();
        var services = new ServiceCollection();
        services.AddSyntaxCircusSeo(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<SearchIndexingOptions>>().Value;

        options.BlockPageMetadata.ShouldBeTrue();
    }

    [Fact]
    public void UseSyntaxCircusSeo_NullApp_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => SeoServiceCollectionExtensions.UseSyntaxCircusSeo(null!));
    }

    [Fact]
    public void UseSyntaxCircusSeo_ReturnsSameApplicationBuilder()
    {
        var services = new ServiceCollection();
        services.AddSyntaxCircusSeo(EmptyConfiguration());
        var app = new ApplicationBuilder(services.BuildServiceProvider());

        var result = app.UseSyntaxCircusSeo();

        result.ShouldBeSameAs(app);
    }
}
