using System.Text.Json;

namespace SyntaxCircus.Blazor.Seo.Tests;

public class SchemasTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void OrganizationSchema_CtorDiscriminator_SerializesContextAndType()
    {
        var schema = new OrganizationSchema("Acme", "https://acme.example", "https://acme.example/logo.png", Founder: new PersonReference("Person", "Jane Doe"));

        var json = JsonSerializer.Serialize(schema, JsonOptions);

        json.ShouldContain("\"@context\":\"https://schema.org\"");
        json.ShouldContain("\"@type\":\"Organization\"");
        json.ShouldContain("\"founder\"");
        json.ShouldContain("\"name\":\"Jane Doe\"");
    }

    [Fact]
    public void OrganizationSchema_DefaultType_IsOrganization()
    {
        new OrganizationSchema("Acme", "https://acme.example", "https://acme.example/logo.png").Type.ShouldBe("Organization");
    }

    [Fact]
    public void BreadcrumbItem_ComputedTypeDiscriminator_SerializesAsListItem()
    {
        var item = new BreadcrumbItem(1, "Home", "https://example.com/");

        var json = JsonSerializer.Serialize(item, JsonOptions);

        item.Type.ShouldBe("ListItem");
        json.ShouldContain("\"@type\":\"ListItem\"");
        json.ShouldContain("\"position\":1");
    }

    [Fact]
    public void QuestionItem_ComputedTypeDiscriminator_SerializesAsQuestion()
    {
        var item = new QuestionItem("What is this?", new AnswerItem("An example."));

        item.Type.ShouldBe("Question");
        item.AcceptedAnswer.Type.ShouldBe("Answer");
    }

    [Fact]
    public void BreadcrumbListSchema_SerializesNestedItemList()
    {
        var schema = new BreadcrumbListSchema(
        [
            new BreadcrumbItem(1, "Home", "https://example.com/"),
            new BreadcrumbItem(2, "Products", "https://example.com/products"),
        ]);

        var json = JsonSerializer.Serialize(schema, JsonOptions);

        schema.Type.ShouldBe("BreadcrumbList");
        json.ShouldContain("\"itemListElement\"");
        json.ShouldContain("\"Home\"");
        json.ShouldContain("\"Products\"");
    }

    [Fact]
    public void FaqPageSchema_SerializesNestedQuestions()
    {
        var schema = new FaqPageSchema([new QuestionItem("Q1?", new AnswerItem("A1."))]);

        var json = JsonSerializer.Serialize(schema, JsonOptions);

        schema.Type.ShouldBe("FAQPage");
        json.ShouldContain("\"mainEntity\"");
        json.ShouldContain("\"acceptedAnswer\"");
    }
}
