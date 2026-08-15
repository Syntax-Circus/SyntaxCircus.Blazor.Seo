namespace SyntaxCircus.Blazor.Seo;

public sealed class SeoUrlBuilder(IOptions<SeoOptions> options, IHttpContextAccessor httpContextAccessor) : ISeoUrlBuilder
{
    public string AbsoluteUrl(string? relativeOrAbsolute)
    {
        if (string.IsNullOrWhiteSpace(relativeOrAbsolute))
        {
            return TrimTrailingSlash(options.Value.BaseUrl);
        }

        if (Uri.TryCreate(relativeOrAbsolute, UriKind.Absolute, out var absolute))
        {
            return absolute.ToString();
        }

        var baseUrl = TrimTrailingSlash(options.Value.BaseUrl);
        var path = relativeOrAbsolute.StartsWith('/') ? relativeOrAbsolute : "/" + relativeOrAbsolute;
        return baseUrl + path;
    }

    public string CanonicalForCurrentRequest(string? overrideRelative)
    {
        if (!string.IsNullOrWhiteSpace(overrideRelative))
        {
            return AbsoluteUrl(overrideRelative);
        }

        var request = httpContextAccessor.HttpContext?.Request;
        if (request is null)
        {
            return TrimTrailingSlash(options.Value.BaseUrl);
        }

        var path = request.Path.HasValue ? request.Path.Value! : "/";
        return AbsoluteUrl(path);
    }

    private static string TrimTrailingSlash(string url) => string.IsNullOrEmpty(url) ? url : url.TrimEnd('/');
}
