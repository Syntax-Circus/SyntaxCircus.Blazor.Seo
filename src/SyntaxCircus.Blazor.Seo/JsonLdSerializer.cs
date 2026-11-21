using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SyntaxCircus.Blazor.Seo;

/// <summary>
/// Serialises objects to JSON that is safe to embed inside a
/// <c>&lt;script type="application/ld+json"&gt;</c> element.
/// </summary>
public static class JsonLdSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false,
    };

    /// <summary>
    /// Serialises <paramref name="data"/> as camelCase JSON (nulls omitted) and escapes
    /// <c>&lt;</c>, <c>&gt;</c>, <c>&amp;</c>, U+2028 and U+2029 as <c>\uXXXX</c> sequences so no
    /// value can close the script element. The result is still valid JSON that parses back to the original values.
    /// </summary>
    public static string Serialize(object data)
    {
        ArgumentNullException.ThrowIfNull(data);
        var json = JsonSerializer.Serialize(data, Options);
        return Escape(json);
    }

    // These characters can only occur inside JSON string values, so replacing them with
    // \uXXXX escapes keeps the document valid and equivalent.
    private const char Backslash = (char)92;

    private static string Escape(string json)
    {
        var sb = new System.Text.StringBuilder(json.Length + 16);
        foreach (var c in json)
        {
            switch (c)
            {
                case '<': sb.Append(Backslash).Append("u003c"); break;
                case '>': sb.Append(Backslash).Append("u003e"); break;
                case '&': sb.Append(Backslash).Append("u0026"); break;
                case (char)0x2028: sb.Append(Backslash).Append("u2028"); break;
                case (char)0x2029: sb.Append(Backslash).Append("u2029"); break;
                default: sb.Append(c); break;
            }
        }
        return sb.ToString();
    }
}
