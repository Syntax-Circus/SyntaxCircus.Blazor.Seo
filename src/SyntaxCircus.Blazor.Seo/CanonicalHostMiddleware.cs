namespace SyntaxCircus.Blazor.Seo;

/// <summary>
/// Issues a 301 redirect when an incoming request's host doesn't match the canonical host
/// configured in <see cref="SeoOptions.BaseUrl"/>. HTTPS itself is enforced separately by
/// <c>UseHttpsRedirection</c>. Localhost/loopback/*.internal hosts are always passed through.
/// </summary>
public sealed class CanonicalHostMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Uri? _canonicalUri;

    public CanonicalHostMiddleware(RequestDelegate next, IOptions<SeoOptions> options)
    {
        _next = next;
        if (Uri.TryCreate(options.Value.BaseUrl, UriKind.Absolute, out var uri))
        {
            _canonicalUri = uri;
        }
    }

    public Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (_canonicalUri is null)
        {
            return _next(context);
        }

        var canonicalHost = _canonicalUri.Host;
        var requestHost = context.Request.Host.Host;
        if (string.Equals(requestHost, canonicalHost, StringComparison.OrdinalIgnoreCase))
        {
            return _next(context);
        }

        if (requestHost.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || requestHost.StartsWith("127.", StringComparison.Ordinal)
            || requestHost.Contains("internal", StringComparison.OrdinalIgnoreCase))
        {
            return _next(context);
        }

        var target = new UriBuilder(_canonicalUri)
        {
            Path = $"{context.Request.PathBase}{context.Request.Path}",
            Query = context.Request.QueryString.HasValue
                ? context.Request.QueryString.Value![1..]
                : string.Empty,
        }.Uri.AbsoluteUri;

        context.Response.Redirect(target, permanent: true);
        return Task.CompletedTask;
    }
}
