namespace SyntaxCircus.Blazor.Seo;

public sealed class SeoOptions
{
    public const string SectionName = "Seo";

    public string BaseUrl { get; set; } = string.Empty;

    public string SiteName { get; set; } = string.Empty;

    public string DefaultDescription { get; set; } = string.Empty;

    public string DefaultOgImage { get; set; } = string.Empty;

    public string DefaultLocale { get; set; } = "en_US";

    public string? TwitterHandle { get; set; }

    public string? LogoUrl { get; set; }

    public IReadOnlyList<string> SameAs { get; set; } = [];
}
