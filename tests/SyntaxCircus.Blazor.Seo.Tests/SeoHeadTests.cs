using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging.Abstractions;
using SyntaxCircus.Blazor.Seo.Components;

namespace SyntaxCircus.Blazor.Seo.Tests;

public class SeoHeadTests
{
    private const string ChildMeta = "<meta property=\"article:tag\" content=\"dotnet\" />";

    private static readonly RenderFragment Child = b => b.AddMarkupContent(0, ChildMeta);

    [Fact]
    public async Task ChildContent_RendersInsideHead_Once()
    {
        var html = await RenderAsync(Child, structuredData: false);

        CountOf(html, "article:tag").ShouldBe(1, html);
        html.ShouldContain(ChildMeta);
    }

    [Fact]
    public async Task ChildContent_KeepsStandardTags()
    {
        var html = await RenderAsync(Child, structuredData: false, ogType: "article");

        html.ShouldContain("name=\"description\"");
        html.ShouldContain("rel=\"canonical\"");
        html.ShouldContain("name=\"robots\"");
        html.ShouldContain("property=\"og:type\" content=\"article\"");
        html.ShouldContain("property=\"og:title\"");
        html.ShouldContain("name=\"twitter:card\"");
    }

    [Fact]
    public async Task ChildContent_RendersAfterTwitterAndBeforeJsonLd()
    {
        var html = await RenderAsync(Child, structuredData: true);

        var twitterImage = html.IndexOf("name=\"twitter:image\"", StringComparison.Ordinal);
        var child = html.IndexOf("article:tag", StringComparison.Ordinal);
        var jsonLd = html.IndexOf("application/ld", StringComparison.Ordinal);

        twitterImage.ShouldBeGreaterThan(-1, html);
        child.ShouldBeGreaterThan(twitterImage, html);
        jsonLd.ShouldBeGreaterThan(child, html);
    }

    [Fact]
    public async Task NoChildContent_OutputUnchanged()
    {
        var html = await RenderAsync(null, structuredData: true);

        html.ShouldNotContain("article:tag");
        string[] expectedOrder =
        [
            "name=\"description\"", "rel=\"canonical\"", "name=\"robots\"",
            "property=\"og:type\"", "property=\"og:site_name\"", "property=\"og:locale\"",
            "property=\"og:title\"", "property=\"og:description\"", "property=\"og:url\"",
            "property=\"og:image\"", "property=\"og:image:alt\"",
            "name=\"twitter:card\"", "name=\"twitter:title\"", "name=\"twitter:description\"",
            "name=\"twitter:image\"", "application/ld",
        ];
        var last = -1;
        foreach (var token in expectedOrder)
        {
            var index = html.IndexOf(token, StringComparison.Ordinal);
            index.ShouldBeGreaterThan(last, $"{token} out of order in {html}");
            last = index;
        }
        var start = html.IndexOf("name=\"twitter:image\"", StringComparison.Ordinal);
        var end = html.IndexOf("<script", StringComparison.Ordinal);
        html[start..end].Split('>').Length.ShouldBe(2, "only the twitter:image tag may sit between it and JSON-LD: " + html);
    }

    private static int CountOf(string haystack, string needle)
    {
        var count = 0;
        for (var i = haystack.IndexOf(needle, StringComparison.Ordinal); i >= 0;
             i = haystack.IndexOf(needle, i + needle.Length, StringComparison.Ordinal))
        {
            count++;
        }
        return count;
    }

    private static async Task<string> RenderAsync(RenderFragment? child, bool structuredData, string? ogType = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<NavigationManager, TestNavigationManager>();
        services.AddSingleton<Microsoft.JSInterop.IJSRuntime, NullJsRuntime>();
        services.AddSingleton<IOptions<SeoOptions>>(Options.Create(new SeoOptions()));
        services.AddSingleton(Substitute.For<ISeoUrlBuilder>());
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, NullLoggerFactory.Instance);
        RenderFragment content = b =>
        {
            b.OpenComponent<SeoHead>(0);
            b.AddComponentParameter(1, nameof(SeoHead.Title), "Post");
            if (ogType is not null)
            {
                b.AddComponentParameter(2, nameof(SeoHead.OgType), ogType);
            }
            if (structuredData)
            {
                b.AddComponentParameter(3, nameof(SeoHead.StructuredData),
                    (IReadOnlyList<object>)[new CreativeWorkSchema("Post", "https://example.com/p", "desc")]);
            }
            if (child is not null)
            {
                b.AddComponentParameter(4, nameof(SeoHead.ChildContent), child);
            }
            b.CloseComponent();
            b.OpenComponent<HeadOutlet>(5);
            b.CloseComponent();
        };
        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<Host>(
                ParameterView.FromDictionary(new Dictionary<string, object?> { ["Content"] = content }));
            return output.ToHtmlString();
        });
    }

    private sealed class Host : ComponentBase
    {
        [Parameter] public RenderFragment? Content { get; set; }
        protected override void BuildRenderTree(RenderTreeBuilder builder) => builder.AddContent(0, Content);
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager() => Initialize("https://example.com/", "https://example.com/");
    }

    private sealed class NullJsRuntime : Microsoft.JSInterop.IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => default;
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => default;
    }
}
