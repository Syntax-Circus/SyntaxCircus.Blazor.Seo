namespace SyntaxCircus.Blazor.Seo;

public interface ISeoUrlBuilder
{
    string AbsoluteUrl(string? relativeOrAbsolute);

    string CanonicalForCurrentRequest(string? overrideRelative);
}
