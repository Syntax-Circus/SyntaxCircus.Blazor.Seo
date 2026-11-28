using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging.Abstractions;
using SyntaxCircus.Blazor.Seo.Components;

namespace SyntaxCircus.Blazor.Seo.Tests;

public class JsonLdSerializerTests
{
    private const string Evil = "x</script><script>alert(1)</script>";

    private static CreativeWorkSchema Schema(string name) =>
        new(name, "https://example.com/p", "desc");

    public static TheoryData<string> Payloads => new()
    {
        Evil,
        "Fish & Chips",
        "line" + (char)0x2028 + "sep" + (char)0x2029 + "para",
        "<!-- <script> -->",
        "caf" + (char)0xE9 + " " + (char)0xFC + "ber",
    };

    [Theory]
    [MemberData(nameof(Payloads))]
    public void Serialize_EscapesAndRoundTrips(string name)
    {
        var json = JsonLdSerializer.Serialize(Schema(name));

        json.ShouldNotContain("<");
        json.ShouldNotContain(">");
        json.ShouldNotContain("&");
        json.ShouldNotContain(((char)0x2028).ToString());
        json.ShouldNotContain(((char)0x2029).ToString());
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("name").GetString().ShouldBe(name);
    }

    [Fact]
    public void Serialize_KeepsNamingAndNonAsciiReadable()
    {
        var json = JsonLdSerializer.Serialize(Schema("caf" + (char)0xE9));

        json.ShouldContain("\"@type\":\"CreativeWork\"");
        json.ShouldContain("caf" + (char)0xE9);
        json.ShouldNotContain("\"image\"");
    }

    [Fact]
    public void Serialize_Null_Throws() =>
        Should.Throw<ArgumentNullException>(() => JsonLdSerializer.Serialize(null!));

    [Fact]
    public async Task JsonLd_Render_HasOneScriptAndRoundTrips()
    {
        var html = await RenderAsync(b =>
        {
            b.OpenComponent<JsonLd>(0);
            b.AddComponentParameter(1, nameof(JsonLd.Data), Schema(Evil));
            b.CloseComponent();
        });

        AssertSafe(html);
    }

    [Fact]
    public async Task SeoHead_StructuredData_HasOneScriptAndRoundTrips()
    {
        var html = await RenderAsync(b =>
        {
            b.OpenComponent<SeoHead>(0);
            b.AddComponentParameter(1, nameof(SeoHead.Title), "Never inline </script>");
            b.AddComponentParameter(2, nameof(SeoHead.StructuredData), (IReadOnlyList<object>)[Schema(Evil)]);
            b.CloseComponent();
            b.OpenComponent<HeadOutlet>(3);
            b.CloseComponent();
        });

        AssertSafe(html);
    }

    private static void AssertSafe(string html)
    {
        CountOf(html, "<script").ShouldBe(1, html);
        CountOf(html, "</script>").ShouldBe(1, html);
        // Literal, not "application/ld&#x2B;json": crawlers that match the raw text miss the encoded form.
        html.ShouldContain("<script type=\"application/ld+json\">");
        html.ShouldNotContain("</script><script>");
        var start = html.IndexOf('>', html.IndexOf("<script", StringComparison.Ordinal)) + 1;
        var end = html.IndexOf("</script>", start, StringComparison.Ordinal);
        using var doc = JsonDocument.Parse(html[start..end]);
        doc.RootElement.GetProperty("name").GetString().ShouldBe(Evil);
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

    private static async Task<string> RenderAsync(Action<RenderTreeBuilder> build)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<Microsoft.AspNetCore.Components.NavigationManager, TestNavigationManager>();
        services.AddSingleton<Microsoft.JSInterop.IJSRuntime, NullJsRuntime>();
        services.AddSingleton<IOptions<SeoOptions>>(Options.Create(new SeoOptions()));
        services.AddSingleton(Substitute.For<ISeoUrlBuilder>());
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, NullLoggerFactory.Instance);
        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<Host>(
                ParameterView.FromDictionary(new Dictionary<string, object?> { ["Content"] = new RenderFragment(build) }));
            return output.ToHtmlString();
        });
    }

    private sealed class Host : ComponentBase
    {
        [Parameter] public RenderFragment? Content { get; set; }
        protected override void BuildRenderTree(RenderTreeBuilder builder) => builder.AddContent(0, Content);
    }

    private sealed class TestNavigationManager : Microsoft.AspNetCore.Components.NavigationManager
    {
        public TestNavigationManager() => Initialize("https://example.com/", "https://example.com/");
    }

    private sealed class NullJsRuntime : Microsoft.JSInterop.IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => default;
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => default;
    }
}
