using System.Text.Json;

namespace SyntaxCircus.Blazor.Seo.Tests;

public class OrganizationSchemaTests
{
    [Fact]
    public void SubOrganization_SerializesAsArrayOfOrganizationReferences()
    {
        var schema = new OrganizationSchema(
            "Syntax Circus",
            "https://www.syntaxcircus.com/",
            "https://www.syntaxcircus.com/logo.png",
            SubOrganization:
            [
                new OrganizationReference("Organization", "Core Musica", "https://www.coremusica.com"),
                new OrganizationReference("Organization", "Sin Forgiver", "https://www.sinforgiver.com"),
            ]);

        using var doc = JsonDocument.Parse(JsonLdSerializer.Serialize(schema));

        var subs = doc.RootElement.GetProperty("subOrganization");
        subs.ValueKind.ShouldBe(JsonValueKind.Array);
        subs.GetArrayLength().ShouldBe(2);
        subs[0].GetProperty("@type").GetString().ShouldBe("Organization");
        subs[0].GetProperty("name").GetString().ShouldBe("Core Musica");
        subs[0].GetProperty("url").GetString().ShouldBe("https://www.coremusica.com");
        subs[1].GetProperty("@type").GetString().ShouldBe("Organization");
        subs[1].GetProperty("name").GetString().ShouldBe("Sin Forgiver");
        subs[1].GetProperty("url").GetString().ShouldBe("https://www.sinforgiver.com");
    }

    [Fact]
    public void ParentOrganization_SerializesAsSingleOrganizationReference()
    {
        var schema = new OrganizationSchema(
            "Core Musica",
            "https://www.coremusica.com",
            "https://www.coremusica.com/logo.png",
            ParentOrganization: new OrganizationReference("Organization", "Syntax Circus", "https://www.syntaxcircus.com/"));

        using var doc = JsonDocument.Parse(JsonLdSerializer.Serialize(schema));

        var parent = doc.RootElement.GetProperty("parentOrganization");
        parent.ValueKind.ShouldBe(JsonValueKind.Object);
        parent.GetProperty("@type").GetString().ShouldBe("Organization");
        parent.GetProperty("name").GetString().ShouldBe("Syntax Circus");
        parent.GetProperty("url").GetString().ShouldBe("https://www.syntaxcircus.com/");
    }

    [Fact]
    public void NoRelations_OmitsBothProperties()
    {
        var schema = new OrganizationSchema("N", "U", "L");

        using var doc = JsonDocument.Parse(JsonLdSerializer.Serialize(schema));

        doc.RootElement.TryGetProperty("subOrganization", out _).ShouldBeFalse();
        doc.RootElement.TryGetProperty("parentOrganization", out _).ShouldBeFalse();
    }

    [Fact]
    public void ExistingPositionalConstruction_StillCompiles()
    {
        var schema = new OrganizationSchema("N", "U", "L", ["s"], new PersonReference("Person", "P"), "D");

        using var doc = JsonDocument.Parse(JsonLdSerializer.Serialize(schema));

        doc.RootElement.GetProperty("sameAs")[0].GetString().ShouldBe("s");
        doc.RootElement.GetProperty("founder").GetProperty("name").GetString().ShouldBe("P");
        doc.RootElement.GetProperty("description").GetString().ShouldBe("D");
    }
}
