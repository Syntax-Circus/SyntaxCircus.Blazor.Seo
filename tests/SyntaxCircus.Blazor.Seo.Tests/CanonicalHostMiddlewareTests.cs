namespace SyntaxCircus.Blazor.Seo.Tests;

public class CanonicalHostMiddlewareTests
{
    private static CanonicalHostMiddleware CreateMiddleware(string baseUrl, RequestDelegate? next = null)
        => new(next ?? (_ => Task.CompletedTask), Options.Create(new SeoOptions { BaseUrl = baseUrl }));

    [Fact]
    public async Task InvokeAsync_NullContext_ThrowsArgumentNullException()
    {
        var middleware = CreateMiddleware("https://example.com");

        await Should.ThrowAsync<ArgumentNullException>(() => middleware.InvokeAsync(null!));
    }

    [Fact]
    public async Task InvokeAsync_InvalidBaseUrl_PassesThrough()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware("not-a-valid-absolute-url", _ => { nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("mismatched.example.com");

        await middleware.InvokeAsync(context);

        nextCalled.ShouldBeTrue();
        context.Response.StatusCode.ShouldBe(200);
    }

    [Fact]
    public async Task InvokeAsync_MatchingHost_PassesThrough()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware("https://example.com", _ => { nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("example.com");

        await middleware.InvokeAsync(context);

        nextCalled.ShouldBeTrue();
    }

    [Theory]
    [InlineData("localhost")]
    [InlineData("127.0.0.1")]
    [InlineData("api.internal")]
    public async Task InvokeAsync_LocalOrInternalHost_PassesThroughEvenIfMismatched(string host)
    {
        var nextCalled = false;
        var middleware = CreateMiddleware("https://example.com", _ => { nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString(host);

        await middleware.InvokeAsync(context);

        nextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task InvokeAsync_MismatchedHost_RedirectsPermanentlyToCanonicalHost()
    {
        var middleware = CreateMiddleware("https://example.com");
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("mismatched.example.com");
        context.Request.Path = "/page";
        context.Request.QueryString = new QueryString("?a=1");

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.ShouldBe(StatusCodes.Status301MovedPermanently);
        context.Response.Headers.Location.ToString().ShouldBe("https://example.com/page?a=1");
    }

    [Fact]
    public async Task InvokeAsync_MismatchedHostNoQueryString_RedirectsWithoutTrailingQuestionMark()
    {
        var middleware = CreateMiddleware("https://example.com");
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("mismatched.example.com");
        context.Request.Path = "/page";

        await middleware.InvokeAsync(context);

        context.Response.Headers.Location.ToString().ShouldBe("https://example.com/page");
    }
}
