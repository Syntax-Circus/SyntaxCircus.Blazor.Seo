using System.Text.Json.Serialization;

// CA1822 (member could be static) is suppressed below on the "@type" discriminator properties:
// System.Text.Json only serializes instance properties, so these must stay instance-level even
// though their value never varies.
#pragma warning disable CA1822

namespace SyntaxCircus.Blazor.Seo;

// Schema.org JSON-LD payload records, rendered via Components.JsonLd. Property names are
// camelCased by the serializer; "@context"/"@type" use JsonPropertyName overrides.

public abstract record SchemaBase(
    [property: JsonPropertyName("@context")] string Context = "https://schema.org",
    [property: JsonPropertyName("@type")] string Type = "Thing");

public sealed record OrganizationSchema(
    string Name,
    string Url,
    string Logo,
    IReadOnlyList<string>? SameAs = null,
    PersonReference? Founder = null,
    string Description = "")
    : SchemaBase(Type: "Organization");

public sealed record PersonReference(
    [property: JsonPropertyName("@type")] string Type,
    string Name,
    string? Url = null);

public sealed record WebSiteSchema(
    string Name,
    string Url,
    string? Description = null)
    : SchemaBase(Type: "WebSite");

public sealed record PersonSchema(
    string Name,
    string Url,
    string? JobTitle = null,
    string? Description = null,
    string? Image = null,
    IReadOnlyList<string>? SameAs = null,
    OrganizationReference? WorksFor = null)
    : SchemaBase(Type: "Person");

public sealed record OrganizationReference(
    [property: JsonPropertyName("@type")] string Type,
    string Name,
    string? Url = null);

public sealed record CreativeWorkSchema(
    string Name,
    string Url,
    string Description,
    string? Image = null,
    OrganizationReference? Publisher = null,
    IReadOnlyList<string>? Keywords = null)
    : SchemaBase(Type: "CreativeWork");

public sealed record SoftwareApplicationSchema(
    string Name,
    string Url,
    string Description,
    string ApplicationCategory,
    string OperatingSystem,
    string? Image = null,
    OrganizationReference? Publisher = null)
    : SchemaBase(Type: "SoftwareApplication");

public sealed record BookSchema(
    string Name,
    string Url,
    string Description,
    PersonReference? Author = null,
    string? BookFormat = null,
    string? WorkExample = null)
    : SchemaBase(Type: "Book");

public sealed record BreadcrumbListSchema(
    IReadOnlyList<BreadcrumbItem> ItemListElement)
    : SchemaBase(Type: "BreadcrumbList");

public sealed record BreadcrumbItem(
    int Position,
    string Name,
    string Item)
{
    [JsonPropertyName("@type")]
    public string Type => "ListItem";
}

public sealed record FaqPageSchema(
    IReadOnlyList<QuestionItem> MainEntity)
    : SchemaBase(Type: "FAQPage");

public sealed record QuestionItem(
    string Name,
    AnswerItem AcceptedAnswer)
{
    [JsonPropertyName("@type")]
    public string Type => "Question";
}

public sealed record AnswerItem(string Text)
{
    [JsonPropertyName("@type")]
    public string Type => "Answer";
}
