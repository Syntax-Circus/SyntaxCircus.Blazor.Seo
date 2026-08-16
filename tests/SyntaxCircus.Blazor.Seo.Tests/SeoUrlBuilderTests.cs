namespace SyntaxCircus.Blazor.Seo.Tests;

public class SeoUrlBuilderTests
{
    private static SeoUrlBuilder CreateBuilder(string baseUrl, IHttpContextAccessor? accessor = null)
        => new(Options.Create(new SeoOptions { BaseUrl = baseUrl }), accessor ?? Substitute.For<IHttpContextAccessor>());

    [Fact]
    public void AbsoluteUrl_BlankInput_ReturnsTrimmedBaseUrl()
    {
        var builder = CreateBuilder("https://example.com/");

        builder.AbsoluteUrl(null).ShouldBe("https://example.com");
        builder.AbsoluteUrl("").ShouldBe("https://example.com");
        builder.AbsoluteUrl("   ").ShouldBe("https://example.com");
    }

    [Fact]
    public void AbsoluteUrl_AlreadyAbsolute_ReturnedViaUri()
    {
        var builder = CreateBuilder("https://example.com");

        builder.AbsoluteUrl("https://other.example/page").ShouldBe("https://other.example/page");
    }

    [Fact]
    public void AbsoluteUrl_RelativeWithLeadingSlash_Combined()
    {
        var builder = CreateBuilder("https://example.com");

        builder.AbsoluteUrl("/page").ShouldBe("https://example.com/page");
    }

    [Fact]
    public void AbsoluteUrl_RelativeWithoutLeadingSlash_Combined()
    {
        var builder = CreateBuilder("https://example.com");

        builder.AbsoluteUrl("page").ShouldBe("https://example.com/page");
    }

    [Fact]
    public void AbsoluteUrl_BaseUrlTrailingSlash_Trimmed()
    {
        var builder = CreateBuilder("https://example.com/");

        builder.AbsoluteUrl("page").ShouldBe("https://example.com/page");
    }

    [Fact]
    public void CanonicalForCurrentRequest_OverrideSupplied_DelegatesToAbsoluteUrl()
    {
        var builder = CreateBuilder("https://example.com");

        builder.CanonicalForCurrentRequest("/override").ShouldBe("https://example.com/override");
    }

    [Fact]
    public void CanonicalForCurrentRequest_NullHttpContext_ReturnsTrimmedBaseUrl()
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);
        var builder = CreateBuilder("https://example.com/", accessor);

        builder.CanonicalForCurrentRequest(null).ShouldBe("https://example.com");
    }

    [Fact]
    public void CanonicalForCurrentRequest_HttpContextPresent_UsesRequestPath()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/current-page";
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(context);
        var builder = CreateBuilder("https://example.com", accessor);

        builder.CanonicalForCurrentRequest(null).ShouldBe("https://example.com/current-page");
    }

    [Fact]
    public void CanonicalForCurrentRequest_HttpContextWithEmptyPath_DefaultsToRoot()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = PathString.Empty;
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(context);
        var builder = CreateBuilder("https://example.com", accessor);

        builder.CanonicalForCurrentRequest(null).ShouldBe("https://example.com/");
    }
}
