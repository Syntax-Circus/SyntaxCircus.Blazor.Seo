namespace SyntaxCircus.Blazor.Seo;

/// <summary>Everything optional — bind whichever fields fit your support flow (mailto, GitHub issues, or both).</summary>
public sealed class SiteSupportOptions
{
    public const string SectionName = "Support";

    public string? Email { get; set; }

    public string? IssueTrackerUrl { get; set; }

    public string? IssueTemplateUrl { get; set; }

    public string? OwnerContactUrl { get; set; }

    public string? OwnerDisplayName { get; set; }
}
